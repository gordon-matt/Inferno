using Ardalis.Result;
using Heroicsoft.NetForge.Results;
using Inferno.Web.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Pages;

public partial class EntityTypeContentBlocks
{
    private IReadOnlyList<EntityTypeContentBlock> blocks;
    private IReadOnlyList<BlockTypeOption> blockTypes = [];
    private IReadOnlyList<ZoneOption> zones = [];
    private string search;
    private Guid? zoneFilter;
    private bool canWrite;

    // Block editors work on a ContentBlock, so the shared fields are edited on one and copied back when saving.
    private ContentBlock editing;
    private string entityType;
    private string entityId;
    private bool isNew;
    private string activeTab = "details";
    private Type editorType;
    private DynamicComponent blockEditor;
    private IReadOnlyDictionary<string, string> errors;
    private bool saving;

    [Inject]
    private IContentBlockAdminService ContentBlockAdminService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        canWrite = await HasPolicyAsync(CmsConstants.Policies.ContentBlocksWrite);
        blockTypes = ContentBlockAdminService.GetBlockTypes();

        var zoneResult = await ContentBlockAdminService.ListZonesAsync(TenantId);
        zones = zoneResult.IsSuccess ? zoneResult.Value : [];

        await LoadAsync();
    }

    private string ZoneName(Guid zoneId) => zones.FirstOrDefault(zone => zone.Id == zoneId)?.Name ?? "—";

    private static string ShortTypeName(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
        {
            return string.Empty;
        }

        string withoutAssembly = typeName.Split(',')[0];
        int lastDot = withoutAssembly.LastIndexOf('.');
        return lastDot < 0 ? withoutAssembly : withoutAssembly[(lastDot + 1)..];
    }

    private async Task LoadAsync()
    {
        var result = await ContentBlockAdminService.ListEntityBlocksAsync(TenantId, search, zoneFilter);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            blocks = [];
            return;
        }

        blocks = result.Value;
    }

    private async Task SearchChangedAsync(string value)
    {
        search = value;
        await LoadAsync();
    }

    private async Task ZoneFilterChangedAsync(ChangeEventArgs args)
    {
        zoneFilter = Guid.TryParse(args.Value?.ToString(), out var zoneId) ? zoneId : null;
        await LoadAsync();
    }

    private void Create()
    {
        errors = null;
        isNew = true;
        editorType = null;
        entityType = null;
        entityId = null;
        editing = new ContentBlock { IsEnabled = true, ZoneId = zoneFilter ?? Guid.Empty };
    }

    private void Edit(EntityTypeContentBlock block, string tab = "details")
    {
        errors = null;
        isNew = false;
        activeTab = tab;
        editorType = blockTypes.FirstOrDefault(option => option.TypeName == block.BlockType)?.EditorType;
        entityType = block.EntityType;
        entityId = block.EntityId;
        editing = new ContentBlock
        {
            Id = block.Id,
            Title = block.Title,
            BlockName = block.BlockName,
            BlockType = block.BlockType,
            ZoneId = block.ZoneId,
            Order = block.Order,
            IsEnabled = block.IsEnabled,
            BlockValues = string.IsNullOrEmpty(block.BlockValues) ? "{}" : block.BlockValues,
            CustomTemplatePath = block.CustomTemplatePath
        };
    }

    private void Close()
    {
        editing = null;
        blockEditor = null;
    }

    private async Task SaveAsync()
    {
        if (saving || !await EnsurePolicyAsync(CmsConstants.Policies.ContentBlocksWrite))
        {
            return;
        }

        if (blockEditor?.Instance is IContentBlockEditor editor)
        {
            editing.BlockValues = editor.Save();
        }

        saving = true;
        try
        {
            var result = await ContentBlockAdminService.SaveEntityBlockAsync(TenantId, new EntityTypeContentBlock
            {
                Id = editing.Id,
                Title = editing.Title,
                EntityType = entityType,
                EntityId = entityId,
                BlockType = editing.BlockType,
                ZoneId = editing.ZoneId,
                Order = editing.Order,
                IsEnabled = editing.IsEnabled,
                BlockValues = editing.BlockValues,
                CustomTemplatePath = editing.CustomTemplatePath
            });

            if (result.Status == ResultStatus.Invalid)
            {
                errors = ErrorText.From(result);
                activeTab = "details";
                return;
            }

            if (!result.IsSuccess)
            {
                Toasts.ShowFailure(result);
                return;
            }

            Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Saved]);
            await LoadAsync();

            if (isNew)
            {
                // A new block starts with default settings, so carry straight on to them.
                blockEditor = null;
                Edit(result.Value, "settings");
                return;
            }

            Close();
        }
        finally
        {
            saving = false;
        }
    }

    private async Task DeleteAsync(EntityTypeContentBlock block)
    {
        bool confirmed = await Dialogs.ConfirmAsync(
            T[InfernoWebLocalizableStrings.Admin.DeleteTitleFormat, block.Title],
            T[InfernoWebLocalizableStrings.Admin.DeleteWarning],
            T[InfernoWebLocalizableStrings.General.Delete],
            isDestructive: true);

        if (!confirmed || !await EnsurePolicyAsync(CmsConstants.Policies.ContentBlocksWrite))
        {
            return;
        }

        var result = await ContentBlockAdminService.DeleteEntityBlockAsync(TenantId, block.Id);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Deleted]);
        await LoadAsync();
    }
}
