namespace Inferno.Plugins;

/// <summary>
/// Convenient base class for plugins. Override <see cref="Install"/> and
/// <see cref="Uninstall"/> to do plugin-specific setup/teardown - the base
/// implementation only flips the installed flag in <c>installedPlugins.json</c>.
/// </summary>
public abstract class BasePlugin : IPlugin
{
    public virtual string GetConfigurationPageUrl() => null;

    public virtual PluginDescriptor PluginDescriptor { get; set; }

    public virtual void Install()
    {
        PluginManager.MarkPluginAsInstalled(PluginDescriptor.SystemName);
        // Keep the in-memory descriptor in sync so consumers don't have to
        // wait for an application restart to pick up the new state.
        PluginDescriptor?.Installed = true;
    }

    public virtual void Uninstall()
    {
        PluginManager.MarkPluginAsUninstalled(PluginDescriptor.SystemName);
        PluginDescriptor?.Installed = false;
    }
}