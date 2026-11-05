using Flatmate;
using Inferno.Security;
using Inferno.Tenants.Entities;
using Inferno.Web;
using Inferno.Web.ContentManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using WebFilez;
using WebFilez.Services;

namespace InfernoCMS.Infrastructure;

/// <summary>Middleware that protects what the libraries mounted on the site leave open.</summary>
public static class PipelineGuards
{
    private const string BROWSER_PATH = "/pickstack/webfilez";

    /// <summary>Headers that cost nothing and close off content sniffing and framing by other sites.</summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // The media browser is framed by the admin pages of the same site, so same-origin framing stays allowed.
        headers["X-Frame-Options"] = "SAMEORIGIN";
        await next();
    });

    /// <summary>
    /// Answers 404 when no tenant owns the host, rather than serving whichever tenant happens to be first.
    /// </summary>
    public static IApplicationBuilder UseKnownTenant(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        if (context.GetTenant<Tenant>() is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await next();
    });

    /// <summary>
    /// The file manager's HTTP API and browser page ship without any sign-in requirement, and an API call without a
    /// scope token reaches the whole media folder. This requires the media permissions, and keeps everyone but the
    /// host administrator inside their own tenant's folder.
    /// </summary>
    public static IApplicationBuilder UseMediaGuard(this WebApplication app)
    {
        string apiPrefix = "/" + app.Services.GetRequiredService<IOptions<WebFilezOptions>>().Value.ApiPrefix.Trim('/');

        return app.Use(async (context, next) =>
        {
            var path = context.Request.Path;
            bool isBrowser = path.Equals(BROWSER_PATH, StringComparison.OrdinalIgnoreCase);
            bool isApi = path.StartsWithSegments(apiPrefix);

            if (!isBrowser && !isApi && !path.StartsWithSegments("/pickstack"))
            {
                await next();
                return;
            }

            var authorization = context.RequestServices.GetRequiredService<IAuthorizationService>();
            bool reading = HttpMethods.IsGet(context.Request.Method)
                || HttpMethods.IsHead(context.Request.Method)
                || HttpMethods.IsOptions(context.Request.Method);

            string policy = reading ? CmsConstants.Policies.MediaRead : CmsConstants.Policies.MediaWrite;
            if (!(await authorization.AuthorizeAsync(context.User, policy)).Succeeded)
            {
                context.Response.StatusCode = context.User.Identity?.IsAuthenticated == true
                    ? StatusCodes.Status403Forbidden
                    : StatusCodes.Status401Unauthorized;
                return;
            }

            bool host = (await authorization.AuthorizeAsync(context.User, StandardPolicies.FullAccess)).Succeeded;
            if (!host)
            {
                string tenantRoot = context.RequestServices.GetRequiredService<IWorkContext>().TenantRoot;

                if (isBrowser)
                {
                    // The page hands out the scope for whichever root it is asked for, so the root is not the caller's to pick.
                    var query = QueryString.Empty;
                    foreach (var pair in context.Request.Query.Where(pair => !pair.Key.Equals("root", StringComparison.OrdinalIgnoreCase)))
                    {
                        query = query.Add(pair.Key, pair.Value.ToString());
                    }

                    context.Request.QueryString = query.Add("root", tenantRoot);
                }
                else if (isApi && !HasScopeToken(context.Request))
                {
                    // Without a scope token the API is not limited to any folder.
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }

            await next();
        });
    }

    private static bool HasScopeToken(HttpRequest request) =>
        !string.IsNullOrEmpty(request.Headers[Constants.ScopeHeaderName].FirstOrDefault())
        || !string.IsNullOrEmpty(request.Query[Constants.ScopeQueryName].FirstOrDefault());
}
