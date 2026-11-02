using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Pages;

public partial class Index
{
    [Inject]
    private IPageService PageService { get; set; }

    [Inject]
    private IPageVersionService PageVersionService { get; set; }

    [Inject]
    private IPageAuthoringService Authoring { get; set; }

    [Inject]
    private IWorkContext WorkContext { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    private NotificationService NotificationService { get; set; }

    [Inject]
    private DialogService DialogService { get; set; }

    private List<Page> pages = [];

    /// <summary>The default-language version of each page, for the address and status columns.</summary>
    private Dictionary<Guid, PageVersion> currentVersions = [];

    private Guid loadVersion = Guid.NewGuid();
    private bool showCreate;
    private bool creating;
    private string newName = string.Empty;
    private Guid? newParentId;
    private string newParentName;
    private string createError;

    private string CreateHeading => newParentName is null
        ? T[InfernoCmsLocalizableStrings.Pages.NewPage].Value
        : T[InfernoCmsLocalizableStrings.Pages.NewSubPageUnder, newParentName].Value;

    protected override void OnInitialized() => Load();

    private void Load()
    {
        int tenantId = WorkContext.CurrentTenant.Id;

        using (var connection = PageService.OpenConnection())
        {
            pages = connection.Query(page => page.TenantId == tenantId).ToList();
        }

        List<PageVersion> versions;
        using (var connection = PageVersionService.OpenConnection())
        {
            versions = connection
                .Query(version => version.TenantId == tenantId && version.CultureCode == null && version.Status != VersionStatus.Archived)
                .ToList();
        }

        currentVersions = versions
            .GroupBy(version => version.PageId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(version => version.Status == VersionStatus.Published)
                    .ThenByDescending(version => version.DateModifiedUtc)
                    .First());

        loadVersion = Guid.NewGuid();
    }

    /// <summary>Starts a new page at the top level, or under <paramref name="parent"/>.</summary>
    private void BeginCreate(Page parent)
    {
        newName = string.Empty;
        newParentId = parent?.Id;
        newParentName = parent?.Name;
        createError = null;
        showCreate = true;
    }

    private void CancelCreate() => showCreate = false;

    private async Task CreateAsync()
    {
        creating = true;
        createError = null;

        try
        {
            var result = await Authoring.CreatePageAsync(newName, newParentId);
            if (!result.Succeeded)
            {
                createError = T[result.ErrorKey].Value;
                return;
            }

            showCreate = false;
            NavigationManager.NavigateTo($"/admin/pages/design/{result.Version.Id}");
        }
        finally
        {
            creating = false;
        }
    }

    private async Task EditAsync(Page page)
    {
        var result = await Authoring.EnsureFirstVersionAsync(page.Id);
        if (!result.Succeeded)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Error", T[result.ErrorKey].Value);
            return;
        }

        NavigationManager.NavigateTo($"/admin/pages/design/{result.Version.Id}");
    }

    private async Task DeleteAsync(Page page)
    {
        bool hasSubPages = pages.Any(candidate => candidate.ParentId == page.Id);
        string question = hasSubPages
            ? T[InfernoCmsLocalizableStrings.Pages.DeleteConfirmWithSubPages].Value
            : T[InfernoCmsLocalizableStrings.Pages.DeleteConfirm].Value;

        if (await DialogService.Confirm(question, page.Name) != true)
        {
            return;
        }

        try
        {
            var result = await Authoring.DeletePageAsync(page.Id);
            if (!result.Succeeded)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", T[result.ErrorKey].Value);
                return;
            }

            Load();
            NotificationService.Notify(NotificationSeverity.Info, "Info", T[InfernoCmsLocalizableStrings.Pages.Deleted].Value);
        }
        catch
        {
            NotificationService.Notify(NotificationSeverity.Error, "Error", T[InfernoCmsLocalizableStrings.Pages.ErrorSaveFailed].Value);
        }
    }
}
