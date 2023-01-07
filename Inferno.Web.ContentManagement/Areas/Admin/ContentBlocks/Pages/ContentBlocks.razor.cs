using Inferno.Models;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Pages
{
    public partial class ContentBlocks
    {
        [Inject]
        private IEnumerable<IContentBlock> BlockTypes { get; set; }

        [Inject]
        private IZoneService ZoneService { get; set; }

        private Type EditorType { get; set; }

        private IEnumerable<IdNamePair<string>> BlockTypesSelectList { get; set; }

        private IEnumerable<IdNamePair<Guid>> ZonesSelectList { get; set; }

        private bool ShowCreateMode { get; set; }

        protected override async Task EditAsync(Guid id)
        {
            await base.EditAsync(id);
            EditorType = BlockTypes.FirstOrDefault(x => x.GetType().FullName == Model.BlockType)?.EditorType;
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            BlockTypesSelectList = BlockTypes
                .Select(x => new
                {
                    x.Name,
                    Type = x.GetTypeFullName()
                })
                .OrderBy(x => x.Name)
                .Select(x => new IdNamePair<string>
                {
                    Id = x.Type,
                    Name = x.Name,
                });

            using var connection = ZoneService.OpenConnection();
            ZonesSelectList = connection.Query()
                .OrderBy(x => x.Name)
                .Select(x => new IdNamePair<Guid>
                {
                    Id = x.Id,
                    Name = x.Name,
                });
        }

        protected override async Task OnValidSumbitAsync()
        {
            if (EditorType is not IContentBlockEditor)
            {
                return;
            }

            Model.BlockValues = (EditorType as IContentBlockEditor)?.Save();
            await base.OnValidSumbitAsync();
        }
    }
}