using Ardalis.Result;
using Heroicsoft.NetForge.Results;
using Inferno.Web.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Pages;

public partial class ContentBlocks
{
    private IReadOnlyList<ContentBlock> blocks;
    private IReadOnlyList<BlockTypeOption> blockTypes = [];
    private IReadOnlyList<ZoneOption> zones = [];
    private string search;
    private Guid? zoneFilter;
    private bool canWrite;
    private bool canManageZones;

    // The page whose blocks are listed, or null for the site-wide blocks.
    private Guid? scopePageId;
    private string scopePageName;
    private Guid? loadedPageId;
    private bool initialized;

    private ContentBlock translating;
    private Type translatingEditorType;
    private bool zonesOpen;

    private ContentBlock editing;
    private bool isNew;
    private string activeTab = "details";
    private Type editorType;
    private DynamicComponent blockEditor;
    private IReadOnlyDictionary<string, string> errors;
    private bool saving;

    /// <summary>Lists only the blocks assigned to this page instead of the site-wide ones.</summary>
    [SupplyParameterFromQuery(Name = "pageId")]
    [Parameter]
    public Guid? PageId { get; set; }

    [Inject]
    private IContentBlockAdminService ContentBlockAdminService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        canWrite = await HasPolicyAsync(CmsConstants.Policies.ContentBlocksWrite);
        canManageZones = await HasPolicyAsync(CmsConstants.Policies.ContentZonesRead);
        blockTypes = ContentBlockAdminService.GetBlockTypes();
        await LoadZonesAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (initialized && PageId == loadedPageId)
        {
            return;
        }

        if (initialized)
        {
            search = null;
            zoneFilter = null;
            Close();
        }

        initialized = true;
        loadedPageId = PageId;
        scopePageId = null;
        scopePageName = null;

        if (PageId.HasValue)
        {
            var page = await ContentBlockAdminService.GetPageNameAsync(TenantId, PageId.Value);
            if (page.IsSuccess)
            {
                scopePageId = PageId;
                scopePageName = page.Value;
            }
            else
            {
                Toasts.ShowFailure(page);
            }
        }

        await LoadAsync();
    }

    private async Task LoadZonesAsync()
    {
        var zoneResult = await ContentBlockAdminService.ListZonesAsync(TenantId);
        zones = zoneResult.IsSuccess ? zoneResult.Value : [];
    }

    private async Task ZonesChangedAsync()
    {
        await LoadZonesAsync();
        if (zoneFilter.HasValue && zones.All(zone => zone.Id != zoneFilter.Value))
        {
            zoneFilter = null;
        }

        await LoadAsync();
    }

    private void OpenTranslator(ContentBlock block)
    {
        translatingEditorType = blockTypes.FirstOrDefault(option => option.TypeName == block.BlockType)?.EditorType;
        translating = block;
    }

    private void CloseTranslator() => translating = null;

    private string Heading => scopePageId is null
        ? T[InfernoCmsLocalizableStrings.ContentBlocks.Title].Value
        : T[InfernoCmsLocalizableStrings.ContentBlocks.PageBlocksTitleFormat, scopePageName].Value;

    private string ZoneName(Guid zoneId) => zones.FirstOrDefault(zone => zone.Id == zoneId)?.Name ?? "—";

    private async Task LoadAsync()
    {
        var result = await ContentBlockAdminService.ListAsync(TenantId, search, zoneFilter, scopePageId);
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
        editing = new ContentBlock { IsEnabled = true, ZoneId = zoneFilter ?? Guid.Empty, PageId = scopePageId };
    }

    private void Edit(ContentBlock block, string tab = "details")
    {
        errors = null;
        isNew = false;
        activeTab = tab;
        editorType = blockTypes.FirstOrDefault(option => option.TypeName == block.BlockType)?.EditorType;

        // Edit a copy, so cancelling leaves the cached block as it was.
        editing = new ContentBlock
        {
            Id = block.Id,
            Title = block.Title,
            BlockName = block.BlockName,
            BlockType = block.BlockType,
            ZoneId = block.ZoneId,
            Order = block.Order,
            IsEnabled = block.IsEnabled,
            BlockValues = block.BlockValues,
            CustomTemplatePath = block.CustomTemplatePath,
            PageId = block.PageId
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
            var result = await ContentBlockAdminService.SaveAsync(TenantId, editing);
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

    private async Task DeleteAsync(ContentBlock block)
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

        var result = await ContentBlockAdminService.DeleteAsync(TenantId, block.Id);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Deleted]);
        await LoadAsync();
    }
}
