using Extenso;
using Inferno.Web.Configuration;
using Inferno.Web.Configuration.Entities;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Components;

/// <summary>
/// Loads <typeparamref name="T"/> from <see cref="Data"/> when the component initializes, and saves it as JSON.
/// An empty value starts from a new <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The settings type.</typeparam>
public abstract class SettingsEditor<T> : ComponentBase, ISettingsEditor
    where T : ISettings, new()
{
    /// <inheritdoc />
    [Parameter]
    public Setting Data { get; set; }

    /// <summary>The settings being edited. Filled from <see cref="Data"/> during initialization.</summary>
    public T Model { get; set; }

    /// <inheritdoc />
    public string Save() => Model.JsonSerialize();

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Model = !string.IsNullOrEmpty(Data?.Value) ? Data.Value.JsonDeserialize<T>() : new T();
    }
}