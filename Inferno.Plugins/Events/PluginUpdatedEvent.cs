namespace Inferno.Plugins.Events;

/// <summary>
/// Raised when a plugin is installed, uninstalled or otherwise has its
/// descriptor reloaded.
/// </summary>
public class PluginUpdatedEvent
{
    public PluginUpdatedEvent(PluginDescriptor plugin)
    {
        Plugin = plugin;
    }

    public PluginDescriptor Plugin { get; private set; }
}