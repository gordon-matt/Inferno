using Inferno.Web.Components;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Localize.Languages;
using Localize.Models;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Components;

/// <summary>
/// Picks a language for one page. A language that already has a version opens in the page designer;
/// a new one is copied from the default language first. The designer is a full page, so this drawer does not edit the content itself.
/// </summary>
public partial class PageTranslator
{
    private IReadOnlyList<LanguageModel> languages = [];
    private IReadOnlyList<string> translated = [];
    private string culture;
    private PageVersion current;
    private bool ready;
    private bool canWrite;
    private bool loadingCulture;
    private bool busy;
    private Guid loadedPageId;

    /// <summary>The page to translate. <see langword="null"/> keeps the drawer closed.</summary>
    [Parameter]
    public Page Item { get; set; }

    /// <summary>A culture to select when the drawer opens. Empty leaves the choice blank.</summary>
    [Parameter]
    public string SelectedCulture { get; set; }

    /// <summary>Raised when the translator asks to close.</summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>Raised after a translation is removed, so the open editor can leave a version that no longer exists.</summary>
    [Parameter]
    public EventCallback OnChanged { get; set; }

    [Inject]
    private IPageAuthoringService Authoring { get; set; }

    [Inject]
    private ILanguageService LanguageService { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Item is null)
        {
            loadedPageId = Guid.Empty;
            ready = false;
            culture = null;
            current = null;
            return;
        }

        if (Item.Id == loadedPageId && ready)
        {
            return;
        }

        loadedPageId = Item.Id;
        ready = false;
        culture = null;
        current = null;

        canWrite = await HasPolicyAsync(CmsConstants.Policies.PagesWrite);

        var listed = await LanguageService.ListAsync();
        languages = listed.IsSuccess ? listed.Value : [];
        await LoadTranslatedAsync();

        culture = string.IsNullOrWhiteSpace(SelectedCulture) ? null : SelectedCulture;
        if (culture is not null)
        {
            await LoadCultureAsync();
        }

        ready = true;
    }

    private bool IsTranslated(string cultureCode) =>
        translated.Contains(cultureCode, StringComparer.OrdinalIgnoreCase);

    private async Task LoadTranslatedAsync()
    {
        translated = Item is null ? [] : await Authoring.ListTranslationCulturesAsync(Item.Id);
    }

    private async Task CultureChangedAsync(ChangeEventArgs args)
    {
        culture = string.IsNullOrWhiteSpace(args.Value?.ToString()) ? null : args.Value.ToString();
        current = null;
        if (culture is null)
        {
            return;
        }

        loadingCulture = true;
        try
        {
            await LoadCultureAsync();
        }
        finally
        {
            loadingCulture = false;
        }
    }

    private async Task LoadCultureAsync()
    {
        current = Item is null || culture is null
            ? null
            : await Authoring.FindCurrentVersionAsync(Item.Id, culture);
    }

    private async Task CreateAsync()
    {
        if (busy || Item is null || culture is null || !canWrite || !await EnsurePolicyAsync(CmsConstants.Policies.PagesWrite))
        {
            return;
        }

        busy = true;
        try
        {
            var ensured = await Authoring.EnsureFirstVersionAsync(Item.Id);
            if (!ensured.Succeeded)
            {
                Toasts.Error(T[ensured.ErrorKey]);
                return;
            }

            var created = await Authoring.CreateTranslationAsync(Item.Id, culture);
            if (!created.Succeeded)
            {
                if (created.ErrorKey == InfernoCmsLocalizableStrings.Pages.ErrorTranslationExists)
                {
                    var existing = await Authoring.FindCurrentVersionAsync(Item.Id, culture);
                    if (existing is not null)
                    {
                        await OpenVersionAsync(existing.Id);
                        return;
                    }
                }

                Toasts.Error(T[created.ErrorKey]);
                return;
            }

            await OpenVersionAsync(created.Version.Id);
        }
        finally
        {
            busy = false;
        }
    }

    private async Task OpenCurrentAsync()
    {
        if (current is null)
        {
            return;
        }

        await OpenVersionAsync(current.Id);
    }

    private async Task OpenVersionAsync(Guid versionId)
    {
        await OnClose.InvokeAsync();
        NavigationManager.NavigateTo($"/admin/pages/design/{versionId}");
    }

    private async Task RemoveAsync()
    {
        if (busy || Item is null || culture is null)
        {
            return;
        }

        bool confirmed = await Dialogs.ConfirmAsync(
            T[InfernoCmsLocalizableStrings.Pages.RemoveTranslation],
            T[InfernoWebLocalizableStrings.Admin.DeleteWarning],
            T[InfernoWebLocalizableStrings.General.Delete],
            isDestructive: true);

        if (!confirmed || !await EnsurePolicyAsync(CmsConstants.Policies.PagesWrite))
        {
            return;
        }

        busy = true;
        try
        {
            var result = await Authoring.DeleteTranslationAsync(Item.Id, culture);
            if (!result.Succeeded)
            {
                Toasts.Error(T[result.ErrorKey]);
                return;
            }

            Toasts.Success(T[InfernoCmsLocalizableStrings.Pages.TranslationRemoved]);
            await LoadTranslatedAsync();
            await LoadCultureAsync();
            await OnChanged.InvokeAsync();
        }
        finally
        {
            busy = false;
        }
    }

    private Task CloseAsync() => OnClose.InvokeAsync();
}
