using Localize.Languages;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Pages;

public partial class Edit
{
    /// <summary>The language dropdown cannot hold null, so the default language is the empty string.</summary>
    private const string DefaultCulture = "";

    [Parameter]
    public Guid PageId { get; set; }

    [Inject]
    private IPageService PageService { get; set; }

    [Inject]
    private ILanguageService LanguageService { get; set; }

    [Inject]
    private IPageAuthoringService Authoring { get; set; }

    [Inject]
    private IPageVersionODataService PageVersionODataService { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    private NotificationService NotificationService { get; set; }

    [Inject]
    private DialogService DialogService { get; set; }

    private Page page;
    private PageVersion version;
    private PageVersionDetails details = new();
    private IReadOnlyList<PageVersion> history = [];
    private IEnumerable<CultureOption> cultures = [];
    private string culture = DefaultCulture;
    private bool saving;

    private string PageName => page?.Name ?? string.Empty;

    protected override async Task OnInitializedAsync()
    {
        using (var connection = PageService.OpenConnection())
        {
            page = connection.Query(candidate => candidate.Id == PageId).FirstOrDefault();
        }

        if (page is null)
        {
            return;
        }

        await LoadCulturesAsync();
        await LoadVersionAsync();
    }

    private async Task LoadCulturesAsync()
    {
        var listed = await LanguageService.ListEnabledAsync();
        var languages = listed.IsSuccess
            ? listed.Value
                .OrderBy(language => language.Name)
                .Select(language => new CultureOption { Value = language.CultureCode, Name = $"{language.Name} ({language.CultureCode})" })
                .ToList()
            : [];

        languages.Insert(0, new CultureOption { Value = DefaultCulture, Name = T[InfernoCmsLocalizableStrings.Pages.DefaultLanguage].Value });
        cultures = languages;
    }

    /// <summary>Loads the version being edited for the chosen language, and its history.</summary>
    private async Task LoadVersionAsync()
    {
        string cultureCode = culture == DefaultCulture ? null : culture;

        if (cultureCode is null)
        {
            // The default language always has a version; pages from before this was automatic get one here.
            var ensured = await Authoring.EnsureFirstVersionAsync(PageId);
            version = ensured.Succeeded ? ensured.Version : null;
        }
        else
        {
            version = await Authoring.FindCurrentVersionAsync(PageId, cultureCode);
        }

        history = version is null ? [] : await Authoring.ListHistoryAsync(PageId, cultureCode);

        var fields = StandardPageContent.Parse(version?.Fields);
        details = new PageVersionDetails
        {
            VersionId = version?.Id ?? Guid.Empty,
            Title = version?.Title,
            Slug = version?.Slug,
            Published = version?.Status == VersionStatus.Published,
            MetaTitle = fields.MetaTitle,
            MetaKeywords = fields.MetaKeywords,
            MetaDescription = fields.MetaDescription,
        };
    }

    private async Task ChangeCultureAsync(string value)
    {
        culture = value ?? DefaultCulture;
        await LoadVersionAsync();
    }

    private async Task CreateTranslationAsync()
    {
        var result = await Authoring.CreateTranslationAsync(PageId, culture);
        if (!result.Succeeded)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Error", T[result.ErrorKey].Value);
            return;
        }

        await LoadVersionAsync();
    }

    private async Task SaveAsync()
    {
        saving = true;

        try
        {
            var pageResult = await Authoring.SavePageAsync(page);
            if (!pageResult.Succeeded)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", T[pageResult.ErrorKey].Value);
                return;
            }

            if (version is not null)
            {
                var detailsResult = await Authoring.SaveDetailsAsync(details);
                if (!detailsResult.Succeeded)
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", T[detailsResult.ErrorKey].Value);
                    return;
                }
            }

            await LoadVersionAsync();
            NotificationService.Notify(NotificationSeverity.Success, "Success", T[InfernoCmsLocalizableStrings.Pages.Saved].Value);
        }
        catch
        {
            NotificationService.Notify(NotificationSeverity.Error, "Error", T[InfernoCmsLocalizableStrings.Pages.ErrorSaveFailed].Value);
        }
        finally
        {
            saving = false;
        }
    }

    private async Task RestoreAsync(PageVersion record)
    {
        try
        {
            if (await DialogService.Confirm(T[InfernoCmsLocalizableStrings.Pages.PageHistoryRestoreConfirm].Value) != true)
            {
                return;
            }

            var response = await PageVersionODataService.RestoreVersionAsync(record.Id);
            if (response.Succeeded)
            {
                await LoadVersionAsync();
                NotificationService.Notify(NotificationSeverity.Info, "Info", T[InfernoCmsLocalizableStrings.Pages.PageHistoryRestoreSuccess].Value);
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", T[InfernoCmsLocalizableStrings.Pages.PageHistoryRestoreError].Value);
            }
        }
        catch
        {
            NotificationService.Notify(NotificationSeverity.Error, "Error", T[InfernoCmsLocalizableStrings.Pages.PageHistoryRestoreError].Value);
        }
    }

    private void EditContent()
    {
        if (version is not null)
        {
            NavigationManager.NavigateTo($"/admin/pages/design/{version.Id}");
        }
    }

    private void GoBack() => NavigationManager.NavigateTo("/admin/pages/index");

    private sealed class CultureOption
    {
        public string Value { get; set; }

        public string Name { get; set; }
    }
}
