namespace Inferno.Plugins.Configuration;

/// <summary>
/// Strongly-typed options bound from the <c>InfernoPluginOptions</c> section
/// of <c>appsettings.json</c>.
/// </summary>
public class InfernoPluginOptions
{
    /// <summary>
    /// When <c>true</c>, every file under <c>~/Plugins/bin</c> is wiped on
    /// application startup. Useful when developing plugins so old shadow
    /// copies don't pin the wrong dll.
    /// </summary>
    public bool ClearPluginShadowDirectoryOnStartup { get; set; }

    /// <summary>
    /// When <c>true</c>, locked plugin assemblies are copied to a unique
    /// timestamped subdirectory and loaded from there. Helps when the OS
    /// has a previous version of the dll locked.
    /// </summary>
    public bool CopyLockedPluginAssembilesToSubdirectoriesOnStartup { get; set; }

    /// <summary>
    /// When <c>true</c>, plugins flagged by Windows as web-downloaded are
    /// loaded with <see cref="System.Reflection.Assembly.UnsafeLoadFrom(string)"/>
    /// rather than the safer load. Only enable if you trust the source.
    /// </summary>
    public bool UseUnsafeLoadAssembly { get; set; }

    /// <summary>
    /// When <c>true</c>, plugin assemblies are shadow-copied into
    /// <c>~/Plugins/bin</c> before being loaded so the originals can be
    /// rebuilt without restarting the app.
    /// </summary>
    public bool UsePluginsShadowCopy { get; set; }
}