namespace Inferno.Plugins;

/// <summary>
/// Front-door for querying the loaded plugin set. Always prefer this over
/// reaching into <see cref="PluginManager.ReferencedPlugins"/> directly so
/// that ACL/tenant filters are applied consistently.
/// </summary>
public interface IPluginFinder
{
    /// <summary>
    /// Returns <c>true</c> if the plugin is available within the given tenant.
    /// Plugins with no explicit <c>LimitedToTenants</c> list are available
    /// everywhere.
    /// </summary>
    bool AuthenticateTenant(PluginDescriptor pluginDescriptor, int? tenantId);

    /// <summary>
    /// Returns <c>true</c> if the supplied user is in at least one of the
    /// plugin's <c>LimitedToUserRoles</c>. Plugins with no role restrictions
    /// are always authorised.
    /// </summary>
    bool AuthorizedForUser(PluginDescriptor pluginDescriptor, string userId);

    IEnumerable<string> GetPluginGroups();

    IEnumerable<T> GetPlugins<T>(
        LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly,
        string userId = null,
        int tenantId = 0,
        string group = null)
        where T : class, IPlugin;

    IEnumerable<PluginDescriptor> GetPluginDescriptors(
        LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly,
        string userId = null,
        int tenantId = 0,
        string group = null);

    IEnumerable<PluginDescriptor> GetPluginDescriptors<T>(
        LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly,
        string userId = null,
        int tenantId = 0,
        string group = null)
        where T : class, IPlugin;

    PluginDescriptor GetPluginDescriptorBySystemName(
        string systemName,
        LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly);

    PluginDescriptor GetPluginDescriptorBySystemName<T>(
        string systemName,
        LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly)
        where T : class, IPlugin;

    /// <summary>
    /// Forces the underlying plugin cache to be rebuilt the next time it is
    /// queried. Useful after install/uninstall to pick up state changes
    /// without restarting the host.
    /// </summary>
    void ReloadPlugins(PluginDescriptor pluginDescriptor);
}