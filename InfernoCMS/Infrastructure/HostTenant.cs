using System.Security.Claims;
using Flatmate;
using Inferno.Tenants.Entities;
using Inferno.Tenants.Services;
using Inferno.Web.Security.Permissions;

namespace InfernoCMS.Infrastructure;

/// <summary>The cookie a host administrator uses to open one tenant's site on this host.</summary>
public static class HostTenantCookie
{
    /// <summary>Cookie name. The value is the tenant id.</summary>
    public const string Name = "Inferno.HostTenant";

    /// <summary>Options for the cookie. HttpOnly, so page script cannot read which tenant was selected.</summary>
    /// <param name="request">The request, used to keep the cookie secure when the call is https.</param>
    /// <returns>The cookie options.</returns>
    public static CookieOptions Options(HttpRequest request) => new()
    {
        HttpOnly = true,
        IsEssential = true,
        SameSite = SameSiteMode.Lax,
        Secure = request.IsHttps,
        Path = "/",
        MaxAge = TimeSpan.FromDays(30)
    };

    /// <summary>Removes the selection. The path matches <see cref="Options"/>.</summary>
    /// <param name="context">The current request.</param>
    public static void Clear(HttpContext context) => context.Response.Cookies.Delete(Name, new CookieOptions
    {
        Path = "/",
        Secure = context.Request.IsHttps,
        SameSite = SameSiteMode.Lax
    });
}

/// <summary>Decides whether the signed-in account is the host administrator.</summary>
public static class HostTenantGate
{
    /// <summary>The account is a host administrator. A tenant user is not, even if a host role was attached to them.</summary>
    /// <param name="context">The current request.</param>
    /// <returns><see langword="true"/> when the account holds host administration.</returns>
    public static async Task<bool> IsHostAdministratorAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        var granted = await context.RequestServices.GetRequiredService<IPermissionStore>().GetGrantedAsync(userId, context.RequestAborted);
        return granted.IsHostAdministrator;
    }
}

/// <summary>Applies a host administrator's selected tenant, and sends them to the chooser when this host belongs to nobody.</summary>
public static class HostTenantMiddleware
{
    /// <summary>
    /// When the signed-in host administrator has selected a tenant, that tenant replaces the one resolved from the host.
    /// Anyone else loses the cookie. A host administrator on a host that matches no tenant is sent to the chooser,
    /// except while they are signing in.
    /// </summary>
    /// <param name="app">The application pipeline.</param>
    /// <returns>The same <paramref name="app"/>, so further middleware can be chained.</returns>
    public static IApplicationBuilder UseHostTenant(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        bool hostAdministrator = await HostTenantGate.IsHostAdministratorAsync(context);
        if (hostAdministrator
            && context.Request.Cookies.TryGetValue(HostTenantCookie.Name, out string raw)
            && int.TryParse(raw, out int tenantId))
        {
            var tenant = await context.RequestServices.GetRequiredService<ITenantService>().FindOneAsync(tenantId);
            if (tenant is null)
            {
                HostTenantCookie.Clear(context);
            }
            else
            {
                context.SetTenantContext(new TenantContext<Tenant>(tenant));
            }
        }
        else if (context.Request.Cookies.ContainsKey(HostTenantCookie.Name))
        {
            HostTenantCookie.Clear(context);
        }

        if (context.GetTenant<Tenant>() is null
            && hostAdministrator
            && HttpMethods.IsGet(context.Request.Method)
            && !IsOpenWithoutTenant(context.Request.Path))
        {
            context.Response.Redirect("/host/tenant");
            return;
        }

        await next();
    });

    /// <summary>Pages that have to answer before a tenant is known: sign-in, the chooser, and the files those pages use.</summary>
    internal static bool IsOpenWithoutTenant(PathString path) =>
        path.StartsWithSegments("/Identity", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/signin-", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/host/tenant", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/img", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/_framework", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/_blazor", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/_content", StringComparison.OrdinalIgnoreCase)
        || path.Equals("/favicon.png", StringComparison.OrdinalIgnoreCase)
        || path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase)
        || path.Equals("/Error", StringComparison.OrdinalIgnoreCase);
}
