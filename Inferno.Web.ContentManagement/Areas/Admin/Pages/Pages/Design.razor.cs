using Inferno.Security.Membership;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Localize.Languages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using PageKrafter.Documents;
using PageKrafter.Rendering;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Pages;

public partial class Design
{
    /// <summary>The language select cannot hold null, so the default language is the empty string.</summary>
    private const string DefaultCulture = "";

    private Guid loadedVersionId;
    private Guid editorKey = Guid.NewGuid();
    private string documentJson;
    private string savedAt;
    private string message;
    private bool found;
    private bool published;
    private bool publishing;
    private bool showDetails;
    private bool showHistory;
    private bool savingDetails;
    private bool restoring;
    private string detailsError;
    private string detailsTab = "visitors";
    private Guid pageId;
    private string cultureCode;
    private Page page = new();

    /// <summary>What the Details drawer edits, so cancelling leaves <see cref="page"/> untouched.</summary>
    private Page pageEdit = new();

    private PageVersionDetails details = new();
    private IReadOnlyList<PageVersion> history = [];
    private IReadOnlyList<CultureOption> cultures = [];
    private List<(Page Page, int Depth)> parentOptions = [];
    private List<InfernoRole> availableRoles = [];
    private HashSet<string> selectedRoleIds = [];

    private string Heading { get; set; } = string.Empty;

    private string CultureValue => cultureCode ?? DefaultCulture;

    /// <summary>The parent select holds text, so the top level is the empty string.</summary>
    private string ParentValue
    {
        get => pageEdit.ParentId?.ToString() ?? string.Empty;
        set => pageEdit.ParentId = Guid.TryParse(value, out var id) ? id : null;
    }

    [Parameter]
    public Guid VersionId { get; set; }

    [Inject]
    private IPageVersionService PageVersionService { get; set; }

    [Inject]
    private IPageService PageService { get; set; }

    [Inject]
    private IPageAuthoringService Authoring { get; set; }

    [Inject]
    private ILanguageService LanguageService { get; set; }

    [Inject]
    private IMembershipService MembershipService { get; set; }

    [Inject]
    private PageRenderer Renderer { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    private ILogger<Design> Logger { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        // Switching language moves to another version of the same page without leaving this component.
        if (loadedVersionId != VersionId)
        {
            loadedVersionId = VersionId;
            showDetails = false;
            showHistory = false;
            savedAt = null;
            await LoadAsync();
        }
    }

    /// <summary>Loads the version, its page and everything the drawers show, and gives the editor the saved content.</summary>
    private async Task LoadAsync()
    {
        found = false;

        PageVersion version;
        using (var connection = PageVersionService.OpenConnection())
        {
            version = connection.Query(candidate => candidate.Id == VersionId).FirstOrDefault();
        }

        if (version is null || version.Status == VersionStatus.Archived || version.TenantId != TenantId)
        {
            message = T[InfernoCmsLocalizableStrings.Pages.ErrorNotFound].Value;
            return;
        }

        List<Page> tenantPages;
        using (var connection = PageService.OpenConnection())
        {
            tenantPages = connection.Query(candidate => candidate.TenantId == version.TenantId).ToList();
        }

        var loadedPage = tenantPages.FirstOrDefault(candidate => candidate.Id == version.PageId);
        if (loadedPage is null)
        {
            message = T[InfernoCmsLocalizableStrings.Pages.ErrorNotFound].Value;
            return;
        }

        page = loadedPage;
        pageId = page.Id;
        cultureCode = version.CultureCode;
        published = version.Status == VersionStatus.Published;
        Heading = string.IsNullOrWhiteSpace(version.Title) ? page.Name : version.Title;

        parentOptions = PageHierarchy.Flatten(tenantPages)
            .Where(option => PageHierarchy.CanBeParent(tenantPages, page.Id, option.Page.Id))
            .ToList();

        await LoadCulturesAsync();
        availableRoles = (await MembershipService.GetAllRolesAsync(TenantId)).ToList();
        await LoadDetailsAsync(version);

        documentJson = StandardPageContent.ForEditor(StandardPageContent.Parse(version.Fields));
        editorKey = Guid.NewGuid();
        found = true;
    }

    private async Task LoadCulturesAsync()
    {
        var listed = await LanguageService.ListEnabledAsync();
        var languages = listed.IsSuccess
            ? listed.Value
                .OrderBy(language => language.Name)
                .Select(language => new CultureOption(language.CultureCode, $"{language.Name} ({language.CultureCode})"))
                .ToList()
            : [];

        languages.Insert(0, new CultureOption(DefaultCulture, T[InfernoCmsLocalizableStrings.Pages.DefaultLanguage].Value));
        cultures = languages;
    }

    /// <summary>Loads what the Details drawer edits, and the save points that can be restored.</summary>
    private async Task LoadDetailsAsync(PageVersion version)
    {
        var fields = StandardPageContent.Parse(version.Fields);
        details = new PageVersionDetails
        {
            VersionId = version.Id,
            Title = version.Title,
            Slug = version.Slug,
            Published = version.Status == VersionStatus.Published,
            MetaTitle = fields.MetaTitle,
            MetaKeywords = fields.MetaKeywords,
            MetaDescription = fields.MetaDescription,
        };

        history = await Authoring.ListHistoryAsync(version.PageId, version.CultureCode);
    }

    private async Task SaveAsync(PageDocument document)
    {
        string html = await Renderer.RenderAsync(document);
        var result = await Authoring.SaveContentAsync(VersionId, PageKrafterJson.Serialize(document), html);

        if (!result.Succeeded)
        {
            Toasts.Error(T[result.ErrorKey]);
            return;
        }

        savedAt = $"{T[InfernoCmsLocalizableStrings.Pages.Saved].Value} {DateTime.Now:HH:mm:ss}";
        await RefreshHistoryAsync();
    }

    private async Task RefreshHistoryAsync() =>
        history = await Authoring.ListHistoryAsync(pageId, cultureCode);

    private async Task SetPublishedAsync(bool publish)
    {
        publishing = true;
        try
        {
            var result = await Authoring.SetPublishedAsync(VersionId, publish);
            if (!result.Succeeded)
            {
                Toasts.Error(T[result.ErrorKey]);
                return;
            }

            published = publish;
            details.Published = publish;
        }
        finally
        {
            publishing = false;
        }
    }

    private void OpenDetails()
    {
        pageEdit = new Page
        {
            Id = page.Id,
            TenantId = page.TenantId,
            ParentId = page.ParentId,
            Name = page.Name,
            IsEnabled = page.IsEnabled,
            Order = page.Order,
            ShowOnMenus = page.ShowOnMenus,
            AccessRestrictions = page.AccessRestrictions,
        };
        selectedRoleIds = PageSecurityHelper.GetRoleIds(page.AccessRestrictions).ToHashSet();
        detailsError = null;
        detailsTab = "visitors";
        showDetails = true;
    }

    private void CloseDetails() => showDetails = false;

    private void ToggleRole(string roleId, bool selected)
    {
        if (selected)
        {
            selectedRoleIds.Add(roleId);
        }
        else
        {
            selectedRoleIds.Remove(roleId);
        }
    }

    private async Task SaveDetailsAsync()
    {
        savingDetails = true;
        detailsError = null;
        try
        {
            pageEdit.AccessRestrictions = PageSecurityHelper.CreateAccessRestrictions(selectedRoleIds);
            var pageResult = await Authoring.SavePageAsync(pageEdit);
            if (!pageResult.Succeeded)
            {
                detailsError = T[pageResult.ErrorKey].Value;
                detailsTab = "page";
                return;
            }

            page = pageEdit;
            details.Published = published;
            var detailsResult = await Authoring.SaveDetailsAsync(details);
            if (!detailsResult.Succeeded)
            {
                detailsError = T[detailsResult.ErrorKey].Value;
                detailsTab = "visitors";
                return;
            }

            Heading = string.IsNullOrWhiteSpace(details.Title) ? page.Name : details.Title.Trim();
            await RefreshHistoryAsync();
            showDetails = false;
            Toasts.Success(T[InfernoCmsLocalizableStrings.Pages.Saved]);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save details for page {PageId}", pageId);
            detailsError = T[InfernoCmsLocalizableStrings.Pages.ErrorSaveFailed].Value;
        }
        finally
        {
            savingDetails = false;
        }
    }

    private async Task OpenHistoryAsync()
    {
        await RefreshHistoryAsync();
        showHistory = true;
    }

    private void CloseHistory() => showHistory = false;

    private async Task RestoreAsync(PageVersion saved)
    {
        bool confirmed = await Dialogs.ConfirmAsync(
            T[InfernoCmsLocalizableStrings.Pages.RestoreVersion],
            T[InfernoCmsLocalizableStrings.Pages.PageHistoryRestoreConfirm],
            T[InfernoCmsLocalizableStrings.Pages.Restore]);

        if (!confirmed)
        {
            return;
        }

        restoring = true;
        try
        {
            var result = await Authoring.RestoreVersionAsync(saved.Id);
            if (!result.Succeeded)
            {
                Toasts.Error(T[result.ErrorKey]);
                return;
            }

            showHistory = false;
            savedAt = null;
            await LoadAsync();
            Toasts.Success(T[InfernoCmsLocalizableStrings.Pages.PageHistoryRestoreSuccess]);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to restore page version {VersionId}", saved.Id);
            Toasts.Error(T[InfernoCmsLocalizableStrings.Pages.PageHistoryRestoreError]);
        }
        finally
        {
            restoring = false;
        }
    }

    private async Task ChangeCultureAsync(string value)
    {
        string culture = string.IsNullOrEmpty(value) ? null : value;

        PageVersion target;
        if (culture is null)
        {
            var ensured = await Authoring.EnsureFirstVersionAsync(pageId);
            target = ensured.Version;
        }
        else
        {
            target = await Authoring.FindCurrentVersionAsync(pageId, culture);
            if (target is null)
            {
                // A new language starts from the default language's content.
                var created = await Authoring.CreateTranslationAsync(pageId, culture);
                if (!created.Succeeded)
                {
                    Toasts.Error(T[created.ErrorKey]);
                    return;
                }

                target = created.Version;
            }
        }

        if (target is not null)
        {
            NavigationManager.NavigateTo($"/admin/pages/design/{target.Id}");
        }
    }

    private sealed record CultureOption(string Value, string Name);
}
