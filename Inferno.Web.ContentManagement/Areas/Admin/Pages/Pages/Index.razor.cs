using Heroicsoft.NetForge.Components.Tables;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Pages;

public partial class Index
{
    private sealed class NewPageForm
    {
        public string Name { get; set; }

        public Guid? ParentId { get; set; }
    }

    private List<Page> pages = [];
    private List<Page> roots = [];

    /// <summary>The default-language version of each page, for the address, status and last edited columns.</summary>
    private Dictionary<Guid, PageVersion> currentVersions = [];

    private TreeTable<Page> tree;
    private string search;
    private bool loaded;
    private bool canWrite;

    private bool creating;
    private bool saving;
    private NewPageForm newPage = new();
    private string createError;
    private List<(Page Page, int Depth)> parentOptions = [];

    [Inject]
    private IPageService PageService { get; set; }

    [Inject]
    private IPageVersionService PageVersionService { get; set; }

    [Inject]
    private IPageAuthoringService Authoring { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    private string CreateHeading
    {
        get
        {
            var parent = pages.FirstOrDefault(page => page.Id == newPage.ParentId);
            return parent is null
                ? T[InfernoCmsLocalizableStrings.Pages.NewPage].Value
                : T[InfernoCmsLocalizableStrings.Pages.NewSubPageUnder, parent.Name].Value;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        canWrite = await HasPolicyAsync(CmsConstants.Policies.PagesWrite);
        Load();
    }

    private IEnumerable<Page> ChildrenOf(Page page) =>
        string.IsNullOrWhiteSpace(search) ? PageHierarchy.ChildrenOf(pages, page.Id) : [];

    private void SearchChanged(string value)
    {
        search = value;
        ApplySearch();
    }

    /// <summary>
    /// Shows the whole tree, or when searching, every matching page as a flat list so that matches
    /// deep in the tree are not hidden under collapsed parents.
    /// </summary>
    private void ApplySearch()
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            roots = PageHierarchy.ChildrenOf(pages, null);
            return;
        }

        string term = search.Trim();
        roots = pages
            .Where(page =>
                page.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
                (currentVersions.GetValueOrDefault(page.Id)?.Slug?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderBy(page => page.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private void Load()
    {
        using (var connection = PageService.OpenConnection())
        {
            pages = connection.Query(page => page.TenantId == TenantId).ToList();
        }

        List<PageVersion> versions;
        using (var connection = PageVersionService.OpenConnection())
        {
            versions = connection
                .Query(version => version.TenantId == TenantId && version.CultureCode == null && version.Status != VersionStatus.Archived)
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

        ApplySearch();
        loaded = true;
    }

    /// <summary>Starts a new page at the top level, or under <paramref name="parent"/>.</summary>
    private void BeginCreate(Page parent)
    {
        newPage = new NewPageForm { ParentId = parent?.Id };
        parentOptions = PageHierarchy.Flatten(pages);
        createError = null;
        creating = true;
    }

    private void CancelCreate() => creating = false;

    private void ParentChanged(ChangeEventArgs args) =>
        newPage.ParentId = Guid.TryParse(args.Value?.ToString(), out var parentId) ? parentId : null;

    private async Task CreateAsync()
    {
        if (saving)
        {
            return;
        }

        saving = true;
        createError = null;
        try
        {
            var result = await Authoring.CreatePageAsync(newPage.Name, newPage.ParentId);
            if (!result.Succeeded)
            {
                createError = T[result.ErrorKey].Value;
                return;
            }

            creating = false;
            NavigationManager.NavigateTo($"/admin/pages/design/{result.Version.Id}");
        }
        finally
        {
            saving = false;
        }
    }

    private async Task EditAsync(Page page)
    {
        var result = await Authoring.EnsureFirstVersionAsync(page.Id);
        if (!result.Succeeded)
        {
            Toasts.Error(T[result.ErrorKey]);
            return;
        }

        NavigationManager.NavigateTo($"/admin/pages/design/{result.Version.Id}");
    }

    private async Task DeleteAsync(Page page)
    {
        bool hasSubPages = pages.Any(candidate => candidate.ParentId == page.Id);
        bool confirmed = await Dialogs.ConfirmAsync(
            T[InfernoWebLocalizableStrings.Admin.DeleteTitleFormat, page.Name],
            hasSubPages ? T[InfernoCmsLocalizableStrings.Pages.DeleteConfirmWithSubPages] : T[InfernoCmsLocalizableStrings.Pages.DeleteConfirm],
            T[InfernoWebLocalizableStrings.General.Delete],
            isDestructive: true);

        if (!confirmed)
        {
            return;
        }

        var result = await Authoring.DeletePageAsync(page.Id);
        if (!result.Succeeded)
        {
            Toasts.Error(T[result.ErrorKey]);
            return;
        }

        Load();
        Toasts.Success(T[InfernoCmsLocalizableStrings.Pages.Deleted]);
    }
}
