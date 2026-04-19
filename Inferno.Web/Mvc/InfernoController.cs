using Dependo;
using Inferno.Web.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Mvc;

public class InfernoController : Controller
{
    private readonly Lazy<IAuthorizationService> authorizationService;

    public IStringLocalizer T { get; private set; }

    public Lazy<IWorkContext> WorkContext { get; private set; }

    public Lazy<SiteSettings> SiteSettings { get; private set; }

    public Lazy<ILogger> Logger { get; private set; }

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

    protected virtual IActionResult RedirectToHomePage() => RedirectToAction("Index", "Home", new { area = string.Empty });
}