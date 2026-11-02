using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Pages;

public partial class Index
{
    [Inject]
    private IPageVersionService PageVersionService { get; set; }

    [Inject]
    private IPageAuthoringService Authoring { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    /// <summary>The default-language version of each listed page, for the address and status columns.</summary>
    private Dictionary<Guid, PageVersion> currentVersions = [];

    private bool showCreate;
    private bool creating;
    private string newName = string.Empty;
    private string createError;

    protected override async Task LoadGridAsync(LoadDataArgs args)
    {
        await base.LoadGridAsync(args);
        LoadCurrentVersions();
    }

    private void BeginCreate()
    {
        newName = string.Empty;
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
            var result = await Authoring.CreatePageAsync(newName);
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

    private void EditPage(Guid pageId) => NavigationManager.NavigateTo($"/admin/pages/edit/{pageId}");

    private async Task EditContentAsync(Guid pageId)
    {
        var result = await Authoring.EnsureFirstVersionAsync(pageId);
        if (!result.Succeeded)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Error", T[result.ErrorKey].Value);
            return;
        }

        NavigationManager.NavigateTo($"/admin/pages/design/{result.Version.Id}");
    }

    private void LoadCurrentVersions()
    {
        var pageIds = (Records ?? []).Select(page => page.Id).ToList();
        if (pageIds.Count == 0)
        {
            currentVersions = [];
            return;
        }

        List<PageVersion> versions;
        using (var connection = PageVersionService.OpenConnection())
        {
            versions = connection
                .Query(version => pageIds.Contains(version.PageId) && version.CultureCode == null && version.Status != VersionStatus.Archived)
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
    }

    private PageVersion CurrentVersionOf(Guid pageId) =>
        currentVersions.GetValueOrDefault(pageId);
}
