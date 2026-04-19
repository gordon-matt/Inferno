using Extenso;
using Inferno.Web.Configuration;
using Inferno.Web.Configuration.Entities;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Components;

public abstract class SettingsEditor<T> : ComponentBase, ISettingsEditor
    where T : ISettings, new()
{
    [Parameter]
    public Setting Data { get; set; }

    public T Model { get; set; }

    public string Save() => Model.JsonSerialize();

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Model = !string.IsNullOrEmpty(Data?.Value) ? Data.Value.JsonDeserialize<T>() : new T();
    }
}