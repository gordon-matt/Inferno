using Inferno.Web.Configuration.Entities;

namespace Inferno.Web.Components;

/// <summary>An editor that loads one stored settings row and returns the JSON to save.</summary>
public interface ISettingsEditor
{
    /// <summary>The stored row. An empty <see cref="Setting.Value"/> starts from a new settings object.</summary>
    Setting Data { get; set; }

    /// <summary>Serializes the edited settings.</summary>
    /// <returns>The JSON to store.</returns>
    string Save();
}