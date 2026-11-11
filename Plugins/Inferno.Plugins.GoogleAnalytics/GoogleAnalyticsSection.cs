using Inferno.Web.Configuration;

namespace Inferno.Plugins.GoogleAnalytics;

/// <summary>The settings tile for this plugin. The form stays on its own page because saving it is not a plain document write.</summary>
public sealed class GoogleAnalyticsSection : ISettingsSection
{
    public string Title => "Google Analytics";

    public string Description => "The measurement ID is sent with every page.";

    public string Icon => "diagram";

    public string Url => GoogleAnalyticsPlugin.SettingsUrl;

    public string Permission => GoogleAnalyticsPolicies.Manage;
}
