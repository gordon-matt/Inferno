using Inferno.Web.Components;
using Inferno.Web.Configuration;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Areas.Admin.Configuration.Pages
{
    public partial class Index
    {
        [Inject]
        private IEnumerable<ISettings> Settings { get; set; }

        private Type EditorType { get; set; }

        protected override async Task EditAsync(Guid id)
        {
            await base.EditAsync(id);
            EditorType = Settings.FirstOrDefault(x => x.GetType().FullName == Model.Type)?.EditorType;
        }

        protected override async Task OnValidSumbitAsync()
        {
            if (EditorType is not ISettingsEditor)
            {
                return;
            }

            Model.Value = (EditorType as ISettingsEditor)?.Save();
            await base.OnValidSumbitAsync();
        }
    }
}