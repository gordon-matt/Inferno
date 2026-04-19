using Dependo;

namespace Inferno.Plugins;

/// <summary>
/// Metadata describing a plugin. Loaded from <c>plugin.json</c> at startup
/// and decorated at runtime with the resolved <see cref="PluginType"/> and
/// <see cref="ReferencedAssembly"/>.
/// </summary>
public class PluginDescriptor : IDescriptor, IComparable<PluginDescriptor>
{
    public PluginDescriptor()
    {
    }

    public PluginDescriptor(Assembly referencedAssembly) : this()
    {
        ReferencedAssembly = referencedAssembly;
    }

    /// <summary>
    /// Resolves an instance of the plugin via the DI container, falling back
    /// to <see cref="DependoResolver.Instance"/>'s reflection-based
    /// <c>ResolveUnregistered</c> when the type isn't registered.
    /// </summary>
    public IPlugin Instance() => Instance<IPlugin>();

    public virtual T Instance<T>() where T : class, IPlugin
    {
        object instance = null;
        try
        {
            instance = DependoResolver.Instance.Resolve(PluginType);
        }
        catch
        {
            // Not registered - fall back to ResolveUnregistered.
        }

        instance ??= DependoResolver.Instance.ResolveUnregistered(PluginType);

        var typedInstance = instance as T;
        if (typedInstance != null)
        {
            typedInstance.PluginDescriptor = this;
        }

        return typedInstance;
    }

    public int CompareTo(PluginDescriptor other) => DisplayOrder != other.DisplayOrder
        ? DisplayOrder.CompareTo(other.DisplayOrder)
        : string.Compare(FriendlyName, other.FriendlyName, StringComparison.Ordinal);

    public override string ToString() => FriendlyName;

    public override bool Equals(object value) =>
        SystemName?.Equals((value as PluginDescriptor)?.SystemName) ?? false;

    public override int GetHashCode() => SystemName?.GetHashCode() ?? 0;

    [JsonProperty(PropertyName = "Group")]
    public virtual string Group { get; set; }

    [JsonProperty(PropertyName = "FriendlyName")]
    public virtual string FriendlyName { get; set; }

    [JsonProperty(PropertyName = "SystemName")]
    public virtual string SystemName { get; set; }

    [JsonProperty(PropertyName = "Version")]
    public virtual string Version { get; set; }

    [JsonProperty(PropertyName = "SupportedVersions")]
    public virtual IList<string> SupportedVersions { get; set; } = [];

    [JsonProperty(PropertyName = "Author")]
    public virtual string Author { get; set; }

    [JsonProperty(PropertyName = "DisplayOrder")]
    public virtual int DisplayOrder { get; set; }

    [JsonProperty(PropertyName = "FileName")]
    public virtual string AssemblyFileName { get; set; }

    [JsonProperty(PropertyName = "Description")]
    public virtual string Description { get; set; }

    /// <summary>
    /// Tenants this plugin is restricted to. Empty means available everywhere.
    /// </summary>
    [JsonProperty(PropertyName = "LimitedToTenants")]
    public virtual IList<int> LimitedToTenants { get; set; } = [];

    /// <summary>
    /// User role IDs this plugin is restricted to. Empty means available to all.
    /// </summary>
    [JsonProperty(PropertyName = "LimitedToUserRoles")]
    public virtual IList<string> LimitedToUserRoles { get; set; } = [];

    [JsonIgnore]
    public virtual bool Installed { get; set; }

    [JsonIgnore]
    public virtual Type PluginType { get; set; }

    [JsonIgnore]
    public virtual FileInfo OriginalAssemblyFile { get; internal set; }

    [JsonIgnore]
    public virtual Assembly ReferencedAssembly { get; internal set; }
}
