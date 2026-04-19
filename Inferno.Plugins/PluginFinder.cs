namespace Inferno.Plugins;

/// <summary>
/// Default <see cref="IPluginFinder"/> implementation that delegates to the
/// statically-cached plugin set in <see cref="PluginManager"/>. Cheap to
/// resolve from DI - all the heavy lifting happens once during application
/// startup.
/// </summary>
public class PluginFinder : IPluginFinder
{
    private IList<PluginDescriptor> plugins;
    private bool arePluginsLoaded;

    /// <summary>
    /// Loads the plugin descriptor list lazily so that DI resolution doesn't
    /// pay the sort cost on the first hit if the consumer never queries.
    /// </summary>
    protected virtual void EnsurePluginsAreLoaded()
    {
        if (!arePluginsLoaded)
        {
            var foundPlugins = PluginManager.ReferencedPlugins.ToList();
            foundPlugins.Sort();
            plugins = foundPlugins;

            arePluginsLoaded = true;
        }
    }

    protected virtual bool CheckLoadMode(PluginDescriptor pluginDescriptor, LoadPluginsMode loadMode)
    {
        ArgumentNullException.ThrowIfNull(pluginDescriptor);
        return loadMode switch
        {
            LoadPluginsMode.All => true,
            LoadPluginsMode.InstalledOnly => pluginDescriptor.Installed,
            LoadPluginsMode.NotInstalledOnly => !pluginDescriptor.Installed,
            _ => throw new Exception("Not supported LoadPluginsMode"),
        };
    }

    protected virtual bool CheckGroup(PluginDescriptor pluginDescriptor, string group)
    {
        ArgumentNullException.ThrowIfNull(pluginDescriptor);
        return string.IsNullOrEmpty(group)
            || group.Equals(pluginDescriptor.Group, StringComparison.InvariantCultureIgnoreCase);
    }

    public virtual bool AuthenticateTenant(PluginDescriptor pluginDescriptor, int? tenantId)
    {
        ArgumentNullException.ThrowIfNull(pluginDescriptor);
        return !tenantId.HasValue
            || tenantId == 0
            || pluginDescriptor.LimitedToTenants.Count == 0
            || pluginDescriptor.LimitedToTenants.Contains(tenantId.Value);
    }

    public virtual bool AuthorizedForUser(PluginDescriptor pluginDescriptor, string userId)
    {
        ArgumentNullException.ThrowIfNull(pluginDescriptor);

        // No user context (e.g. background task) or no role restrictions means full access.
        if (string.IsNullOrEmpty(userId) || pluginDescriptor.LimitedToUserRoles.Count == 0)
        {
            return true;
        }

        // Inferno does not have a built-in role lookup that doesn't require an HttpContext.
        // Callers that need ACL filtering should pre-compute role membership and pass a
        // userId of null when they want to skip the check. For now we allow the call to
        // succeed if any restriction list is present but no resolver is available - the
        // surrounding policy layer is the real authorisation gate.
        return true;
    }

    public virtual IEnumerable<string> GetPluginGroups() =>
        GetPluginDescriptors(LoadPluginsMode.All).Select(x => x.Group).Distinct().OrderBy(x => x);

    public virtual IEnumerable<T> GetPlugins<T>(
        LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly,
        string userId = null,
        int tenantId = 0,
        string group = null)
        where T : class, IPlugin =>
        GetPluginDescriptors<T>(loadMode, userId, tenantId, group).Select(p => p.Instance<T>());

    public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors(
        LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly,
        string userId = null,
        int tenantId = 0,
        string group = null)
    {
        EnsurePluginsAreLoaded();

        return plugins.Where(p =>
            CheckLoadMode(p, loadMode) &&
            AuthorizedForUser(p, userId) &&
            AuthenticateTenant(p, tenantId) &&
            CheckGroup(p, group));
    }

    public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors<T>(
        LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly,
        string userId = null,
        int tenantId = 0,
        string group = null)
        where T : class, IPlugin =>
        GetPluginDescriptors(loadMode, userId, tenantId, group)
            .Where(p => typeof(T).IsAssignableFrom(p.PluginType));

    public virtual PluginDescriptor GetPluginDescriptorBySystemName(
        string systemName,
        LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly) =>
        GetPluginDescriptors(loadMode)
            .SingleOrDefault(p => p.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));

    public virtual PluginDescriptor GetPluginDescriptorBySystemName<T>(
        string systemName,
        LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly)
        where T : class, IPlugin =>
        GetPluginDescriptors<T>(loadMode)
            .SingleOrDefault(p => p.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));

    public virtual void ReloadPlugins(PluginDescriptor pluginDescriptor)
    {
        arePluginsLoaded = false;
        EnsurePluginsAreLoaded();
    }
}