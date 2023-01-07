using Inferno.Models;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
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

        protected override void Create()
        {
            Model = new ContentBlock();
            ShowEditMode = false;
            ShowCreateMode = true;
        }

        protected override async Task EditAsync(Guid id)
        {
            ShowCreateMode = false;
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
                    Type = x.GetType().FullName
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
                .ToList()
                .Select(x => new IdNamePair<Guid>
                {
                    Id = x.Id,
                    Name = x.Name,
                });
        }

        protected override async Task OnValidSumbitAsync()
        {
            if (!ShowCreateMode)
            {
                if (!typeof(IContentBlockEditor).IsAssignableFrom(EditorType))
                {
                    return;
                }

                var editor = (IContentBlockEditor)Activator.CreateInstance(EditorType);
                Model.BlockValues = editor?.Save();
            }

            await base.OnValidSumbitAsync();
            ShowCreateMode = false;
        }

        protected override void Cancel()
        {
            base.Cancel();
            ShowCreateMode = false;
        }
    }
}