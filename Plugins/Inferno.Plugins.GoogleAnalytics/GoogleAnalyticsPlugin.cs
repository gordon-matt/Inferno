using Inferno.Plugins.GoogleAnalytics.ContentBlocks;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;
using Inferno.Web.Dashboard;
using Inferno.Web.Navigation;
using Inferno.Web.Security.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Pluginz;

namespace Inferno.Plugins.GoogleAnalytics;

public sealed class GoogleAnalyticsPlugin : PluginBase
{
    public const string SettingsUrl = "/admin/google-analytics";

    public override string? ConfigurationUrl => SettingsUrl;

    public override void ConfigureServices(IServiceCollection services, PluginContext context)
    {
        services.AddTransient<IContentBlock, GoogleAnalyticsBlock>();
        services.AddSingleton<IPermissionProvider, GoogleAnalyticsPermissionProvider>();
        services.AddSingleton<INavigationProvider, GoogleAnalyticsNavigationProvider>();
        services.AddSingleton<IDashboardContributor, GoogleAnalyticsDashboardContributor>();
        services.AddScoped<IGoogleAnalyticsSettingsStore, GoogleAnalyticsSettingsStore>();
        services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();
        services.AddHttpClient(GoogleAnalyticsClient.HttpClientName);
        services.AddSingleton<IGoogleAnalyticsClient, GoogleAnalyticsClient>();
    }
}
