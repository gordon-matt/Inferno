using Dependo;
using Inferno.Web.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Mvc;

/// <summary>Base controller. Localizer, work context, site settings, and logger are resolved from Dependo on first use.</summary>
public class InfernoController : Controller
{
    private readonly Lazy<IAuthorizationService> authorizationService;

    /// <summary>The localizer, resolved when the controller is created.</summary>
    public IStringLocalizer T { get; private set; }

    /// <summary>The work context, resolved on first use.</summary>
    public Lazy<IWorkContext> WorkContext { get; private set; }

    /// <summary>The current tenant's site settings, resolved on first use.</summary>
    public Lazy<SiteSettings> SiteSettings { get; private set; }

    /// <summary>A logger for the concrete controller type, created on first use.</summary>
    public Lazy<ILogger> Logger { get; private set; }

    /// <summary>Resolves the localizer and prepares the lazy services.</summary>
    protected InfernoController()
    {
        T = DependoResolver.Instance.Resolve<IStringLocalizer>();
        WorkContext = new Lazy<IWorkContext>(() => DependoResolver.Instance.Resolve<IWorkContext>());
        SiteSettings = new Lazy<SiteSettings>(() => DependoResolver.Instance.Resolve<SiteSettings>());

        Logger = new Lazy<ILogger>(() =>
        {
            var loggerFactory = DependoResolver.Instance.Resolve<ILoggerFactory>();
            return loggerFactory.CreateLogger(GetType());
        });

        authorizationService = new Lazy<IAuthorizationService>(() => DependoResolver.Instance.Resolve<IAuthorizationService>());
    }

    /// <summary>Whether the current user satisfies <paramref name="policyName"/>. An empty policy is allowed.</summary>
    /// <param name="policyName">The policy to check. Null or empty returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> when the policy succeeds.</returns>
    protected virtual async Task<bool> AuthorizeAsync(string policyName)
    {
        if (string.IsNullOrEmpty(policyName))
        {
            return true;
        }

        if (authorizationService == null)
        {
            return false;
        }

        var result = await authorizationService.Value.AuthorizeAsync(User, policyName);
        return result.Succeeded;
    }

    /// <summary>Redirects to the home page outside any area.</summary>
    /// <returns>The redirect.</returns>
    protected virtual IActionResult RedirectToHomePage() => RedirectToAction("Index", "Home", new { area = string.Empty });
}