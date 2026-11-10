using Dependo;
using Inferno.Web;
using Inferno.Web.Navigation;
using Microsoft.Extensions.Localization;

namespace Inferno.Plugins.GoogleAnalytics;

public sealed class GoogleAnalyticsNavigationProvider : INavigationProvider
{
    public string MenuName => InfernoWebConstants.Areas.Admin;

    public void GetNavigation(NavigationBuilder builder)
    {
        var localizer = DependoResolver.Instance.Resolve<IStringLocalizer>();
        builder.Add(localizer[InfernoWebLocalizableStrings.General.Configuration], "2", menu =>
        {
            menu.Add("Google Analytics", "6", item => item
                .Url(GoogleAnalyticsPlugin.SettingsUrl)
                .Icon("fas fa-chart-line")
                .Permission(GoogleAnalyticsPolicies.Manage));
        });
    }
}
