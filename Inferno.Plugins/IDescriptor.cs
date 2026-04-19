namespace Inferno.Plugins;

/// <summary>
/// Represents descriptor of an application extension (plugin or theme).
/// </summary>
public interface IDescriptor
{
    string SystemName { get; set; }

    string FriendlyName { get; set; }
}