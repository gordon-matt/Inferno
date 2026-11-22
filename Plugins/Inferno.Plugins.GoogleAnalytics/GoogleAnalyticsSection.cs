using Inferno.Web.Configuration;

namespace Inferno.Plugins.GoogleAnalytics;

/// <summary>The settings tile for this plugin. The form stays on its own page because saving it is not a plain document write.</summary>
public sealed class GoogleAnalyticsSection : ISettingsSection
{
    /// <inheritdoc />
    public string Title => "Google Analytics";

    /// <inheritdoc />
    public string Description => "The measurement ID is sent with every page.";

    /// <inheritdoc />
    public string Icon => "diagram";

    /// <inheritdoc />
    public string Url => GoogleAnalyticsPlugin.SettingsUrl;

    /// <inheritdoc />
    public string Permission => GoogleAnalyticsPolicies.Manage;
}
