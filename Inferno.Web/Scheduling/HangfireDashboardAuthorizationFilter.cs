using Hangfire.Dashboard;
using Inferno.Security;
using Inferno.Tasks.Scheduling;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Web.Scheduling;

/// <summary>
/// The dashboard can trigger and delete every job, including ones that walk all tenants, so only the
/// host administrator (the Administrators role with no tenant) may open it.
/// </summary>
public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => Allows(context.GetHttpContext());

    /// <summary>
    /// Runs before Hangfire. Hangfire turns a failed <see cref="Authorize"/> into a bare 401, which hides the sign-in page.
    /// </summary>
    public static async Task RedirectUnlessAllowed(HttpContext http, Func<Task> next)
    {
        if (!http.Request.Path.StartsWithSegments(ScheduledJobs.DashboardPath))
        {
            await next();
            return;
        }

        if (http.User?.Identity?.IsAuthenticated != true)
        {
            await http.ChallengeAsync();
            return;
        }

        if (!Allows(http))
        {
            await http.ForbidAsync();
            return;
        }

        await next();
    }

    private static bool Allows(HttpContext http)
    {
        if (http.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var authorization = http.RequestServices.GetRequiredService<IAuthorizationService>();
        return authorization.AuthorizeAsync(http.User, StandardPolicies.FullAccess).GetAwaiter().GetResult().Succeeded;
    }
}
