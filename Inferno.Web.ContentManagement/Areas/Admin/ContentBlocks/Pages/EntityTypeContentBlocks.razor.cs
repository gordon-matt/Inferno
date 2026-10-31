using Inferno.Models;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Pages;

public partial class EntityTypeContentBlocks
{
    [Inject]
    private IEnumerable<IContentBlock> BlockTypes { get; set; }

    [Inject]
    private IZoneService ZoneService { get; set; }

    private Type EditorType { get; set; }

    private DynamicComponent BlockEditor { get; set; }

    private ContentBlock EditorData { get; set; }

    private IEnumerable<IdNamePair<string>> BlockTypesSelectList { get; set; }

    private IEnumerable<IdNamePair<Guid>> ZonesSelectList { get; set; }

    private bool ShowCreateMode { get; set; }

    protected override void Create()
    {
        Model = new EntityTypeContentBlock { BlockValues = "{}" };
        EditorData = null;
        ShowEditMode = false;
        ShowCreateMode = true;
    }

    protected override async Task EditAsync(Guid id)
    {
        ShowCreateMode = false;
        await base.EditAsync(id);
        EditorType = BlockTypes.FirstOrDefault(x => x.GetType().FullName == Model.BlockType)?.EditorType;
        EditorData = new ContentBlock
        {
            Id = Model.Id,
            Title = Model.Title,
            BlockType = Model.BlockType,
            BlockValues = string.IsNullOrEmpty(Model.BlockValues) ? "{}" : Model.BlockValues,
            ZoneId = Model.ZoneId,
            Order = Model.Order,
            IsEnabled = Model.IsEnabled,
            CustomTemplatePath = Model.CustomTemplatePath
        };
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        BlockTypesSelectList = BlockTypes
            .Select(x => new IdNamePair<string>
            {
                Id = x.GetType().FullName,
                Name = x.Name
            })
            .OrderBy(x => x.Name);

        using var connection = ZoneService.OpenConnection();
        ZonesSelectList = connection.Query()
            .OrderBy(x => x.Name)
            .ToList()
            .Select(x => new IdNamePair<Guid>
            {
                Id = x.Id,
                Name = x.Name
            });
    }

    protected override async Task OnValidSumbitAsync()
    {
        if (!ShowCreateMode && BlockEditor?.Instance is IContentBlockEditor editor)
        {
            Model.BlockValues = editor.Save();
        }

        if (string.IsNullOrEmpty(Model.BlockValues))
        {
            Model.BlockValues = "{}";
        }

        await base.OnValidSumbitAsync();
        ShowCreateMode = false;
    }

    protected override void Cancel()
    {
        base.Cancel();
        ShowCreateMode = false;
        EditorData = null;
    }
}
