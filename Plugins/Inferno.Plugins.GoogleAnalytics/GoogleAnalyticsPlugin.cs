using Inferno.Plugins.GoogleAnalytics.ContentBlocks;
using Inferno.Web.Configuration;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;
using Inferno.Web.Dashboard;
using Inferno.Web.Security.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Pluginz;

namespace Inferno.Plugins.GoogleAnalytics;

/// <summary>Registers the Analytics content block, permissions, dashboard widgets, settings page, and the Data API client.</summary>
public sealed class GoogleAnalyticsPlugin : PluginBase
{
    /// <summary>The admin page that edits the measurement id, property, and service account.</summary>
    public const string SettingsUrl = "/admin/google-analytics";

    /// <inheritdoc />
    public override string? ConfigurationUrl => SettingsUrl;

    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services, PluginContext context)
    {
        services.AddTransient<IContentBlock, GoogleAnalyticsBlock>();
        services.AddSingleton<IPermissionProvider, GoogleAnalyticsPermissionProvider>();
        services.AddSingleton<IDashboardContributor, GoogleAnalyticsDashboardContributor>();
        services.AddScoped<ISettingsSection, GoogleAnalyticsSection>();
        services.AddScoped<IGoogleAnalyticsSettingsStore, GoogleAnalyticsSettingsStore>();
        services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();
        services.AddHttpClient(GoogleAnalyticsClient.HttpClientName);
        services.AddSingleton<IGoogleAnalyticsClient, GoogleAnalyticsClient>();
    }
}
