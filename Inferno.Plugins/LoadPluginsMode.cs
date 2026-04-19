namespace Inferno.Plugins;

/// <summary>
/// Filters which plugins are returned by <see cref="IPluginFinder"/>.
/// </summary>
public enum LoadPluginsMode
{
    /// <summary>Include both installed and uninstalled plugins.</summary>
    All = 0,

    /// <summary>Only return plugins that have been installed.</summary>
    InstalledOnly = 10,

    /// <summary>Only return plugins that have not yet been installed.</summary>
    NotInstalledOnly = 20,
}