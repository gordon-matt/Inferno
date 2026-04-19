namespace Inferno.Plugins;

/// <summary>
/// Marker interface for a plugin. A plugin is the entry-point class that the
/// host CMS instantiates to install/uninstall the plugin and to surface a
/// configuration page.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// URL of the plugin configuration page (or <c>null</c> if the plugin is
    /// not user-configurable).
    /// </summary>
    string GetConfigurationPageUrl();

    /// <summary>
    /// Descriptor (loaded from <c>plugin.json</c>) that describes this plugin.
    /// </summary>
    PluginDescriptor PluginDescriptor { get; set; }

    void Install();

    void Uninstall();
}