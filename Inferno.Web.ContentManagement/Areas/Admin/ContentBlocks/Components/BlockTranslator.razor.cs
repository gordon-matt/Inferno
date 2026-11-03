using Heroicsoft.NetForge.Results;
using Inferno.Web.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using Localize.Languages;
using Localize.Models;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;

/// <summary>Edits a block's content in one language at a time. Each language keeps its own copy of the block's settings.</summary>
public partial class BlockTranslator
{
    private IReadOnlyList<LanguageModel> languages = [];
    private IReadOnlyList<string> translated = [];
    private string culture;
    private bool exists;

    // The editor works on a copy so cancelling never touches the block as listed on the page.
    private ContentBlock translating;
    private DynamicComponent blockEditor;
    private int generation;
    private bool saving;
    private Guid loadedBlockId;

    /// <summary>The block to translate; null keeps the drawer closed.</summary>
    [Parameter]
    public ContentBlock Block { get; set; }

    [Parameter]
    public Type EditorType { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Inject]
    private IContentBlockAdminService ContentBlockAdminService { get; set; }

    [Inject]
    private ILanguageService LanguageService { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Block is null)
        {
            loadedBlockId = Guid.Empty;
            return;
        }

        if (Block.Id == loadedBlockId)
        {
            return;
        }

        loadedBlockId = Block.Id;
        culture = null;
        translating = null;
        exists = false;

        var listed = await LanguageService.ListAsync();
        languages = listed.IsSuccess ? listed.Value : [];
        await LoadTranslatedAsync();
    }

    private bool IsTranslated(string cultureCode) =>
        translated.Contains(cultureCode, StringComparer.OrdinalIgnoreCase);

    private async Task LoadTranslatedAsync()
    {
        var result = await ContentBlockAdminService.ListTranslationCulturesAsync(TenantId, Block.Id);
        translated = result.IsSuccess ? result.Value : [];
    }

    private async Task CultureChangedAsync(ChangeEventArgs args)
    {
        culture = string.IsNullOrWhiteSpace(args.Value?.ToString()) ? null : args.Value.ToString();
        await LoadCultureAsync();
    }

    private async Task LoadCultureAsync()
    {
        translating = null;
        blockEditor = null;
        if (culture is null)
        {
            return;
        }

        var result = await ContentBlockAdminService.GetTranslationAsync(TenantId, Block.Id, culture);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            culture = null;
            return;
        }

        exists = result.Value.Exists;
        generation++;
        translating = new ContentBlock
        {
            Id = Block.Id,
            Title = Block.Title,
            BlockType = Block.BlockType,
            BlockValues = result.Value.BlockValues,
        };
    }

    private async Task SaveAsync()
    {
        if (saving || culture is null || !await EnsurePolicyAsync(CmsConstants.Policies.ContentBlocksWrite))
        {
            return;
        }

        string blockValues = blockEditor?.Instance is IContentBlockEditor editor ? editor.Save() : translating.BlockValues;

        saving = true;
        try
        {
            var result = await ContentBlockAdminService.SaveTranslationAsync(TenantId, Block.Id, culture, blockValues);
            if (!result.IsSuccess)
            {
                Toasts.ShowFailure(result);
                return;
            }

            exists = true;
            Toasts.Success(T[InfernoCmsLocalizableStrings.ContentBlocks.TranslationSaved]);
            await LoadTranslatedAsync();
        }
        finally
        {
            saving = false;
        }
    }

    private async Task RemoveAsync()
    {
        bool confirmed = await Dialogs.ConfirmAsync(
            T[InfernoCmsLocalizableStrings.ContentBlocks.RemoveTranslation],
            T[InfernoWebLocalizableStrings.Admin.DeleteWarning],
            T[InfernoWebLocalizableStrings.General.Delete],
            isDestructive: true);

        if (!confirmed || !await EnsurePolicyAsync(CmsConstants.Policies.ContentBlocksWrite))
        {
            return;
        }

        var result = await ContentBlockAdminService.DeleteTranslationAsync(TenantId, Block.Id, culture);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        Toasts.Success(T[InfernoCmsLocalizableStrings.ContentBlocks.TranslationRemoved]);
        await LoadTranslatedAsync();
        await LoadCultureAsync();
    }

    private Task CloseAsync() => OnClose.InvokeAsync();
}
