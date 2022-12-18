using Extenso;
using Inferno.Web.Configuration;
using Inferno.Web.Configuration.Entities;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Components
{
    public abstract class SettingsEditor<T> : ComponentBase, ISettingsEditor
        where T : ISettings, new()
    {
        [Parameter]
        public Setting Data { get; set; }

        public T Model { get; set; }

        public string Save()
        {
            return Model.JsonSerialize();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (!string.IsNullOrEmpty(Data?.Value))
            {
                Model = Data.Value.JsonDeserialize<T>();
            }
            else
            {
                Model = new T();
            }
        }
    }
}