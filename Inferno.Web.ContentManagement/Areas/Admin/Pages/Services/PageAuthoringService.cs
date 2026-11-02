using System.Text.RegularExpressions;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Media;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;

/// <summary>
/// The outcome of an authoring operation. <see cref="ErrorKey"/> is a localisation key from
/// <see cref="InfernoCmsLocalizableStrings.Pages"/>, ready to show to the user.
/// </summary>
public sealed record PageAuthoringResult(bool Succeeded, string ErrorKey = null, PageVersion Version = null)
{
    public static PageAuthoringResult Success(PageVersion version = null) => new(true, null, version);

    public static PageAuthoringResult Failure(string errorKey) => new(false, errorKey);
}

/// <summary>What the page screen edits for one version. Null members are left unchanged.</summary>
public sealed class PageVersionDetails
{
    public Guid VersionId { get; set; }

    public string Title { get; set; }

    public string Slug { get; set; }

    public bool Published { get; set; }

    /// <summary>Search settings.</summary>
    public string MetaTitle { get; set; }

    public string MetaKeywords { get; set; }

    public string MetaDescription { get; set; }
}

/// <summary>
/// Creates pages and saves their content. Versions are never created by hand: a page starts with one,
/// and every time its title, address or content is saved the previous state is kept automatically as a save point.
/// </summary>
public interface IPageAuthoringService
{
    /// <summary>Creates a page together with its first (draft) version in the default language. A page with a parent becomes a sub-page.</summary>
    Task<PageAuthoringResult> CreatePageAsync(string name, Guid? parentId = null);

    /// <summary>
    /// Saves the page's own settings: name, order, where it sits in the page tree, whether it is switched on
    /// and shown on menus, and which roles may see it.
    /// </summary>
    Task<PageAuthoringResult> SavePageAsync(Page page);

    /// <summary>Deletes a page with its versions and content blocks. Its sub-pages move up to its parent.</summary>
    Task<PageAuthoringResult> DeletePageAsync(Guid pageId);

    /// <summary>
    /// Puts a save point's title, address and content back into the version being edited. The state it replaces
    /// is kept as a new save point, so a restore can itself be undone.
    /// </summary>
    Task<PageAuthoringResult> RestoreVersionAsync(Guid archivedVersionId);

    /// <summary>
    /// The default-language version of a page, created from the page name if the page has none yet
    /// (for example a page added before versions were handled automatically).
    /// </summary>
    Task<PageAuthoringResult> EnsureFirstVersionAsync(Guid pageId);

    /// <summary>The version being edited for a language: the published one, or else the draft.</summary>
    Task<PageVersion> FindCurrentVersionAsync(Guid pageId, string cultureCode = null);

    /// <summary>The kept earlier versions for a language, newest first.</summary>
    Task<IReadOnlyList<PageVersion>> ListHistoryAsync(Guid pageId, string cultureCode = null);

    /// <summary>Adds a language to a page, starting from the default language's content.</summary>
    Task<PageAuthoringResult> CreateTranslationAsync(Guid pageId, string cultureCode);

    /// <summary>Saves the title, address, status and search settings.</summary>
    Task<PageAuthoringResult> SaveDetailsAsync(PageVersionDetails details);

    /// <summary>Makes a version visible to visitors, or turns it back into a draft.</summary>
    Task<PageAuthoringResult> SetPublishedAsync(Guid versionId, bool published);

    /// <summary>Saves the designed content (the editor document and the HTML visitors see).</summary>
    Task<PageAuthoringResult> SaveContentAsync(Guid versionId, string documentJson, string html);
}

public class PageAuthoringService : IPageAuthoringService
{
    private readonly IPageService pageService;
    private readonly IPageVersionService versionService;
    private readonly IContentBlockService contentBlockService;
    private readonly IWorkContext workContext;
    private readonly IAuthorizationService authorizationService;
    private readonly AuthenticationStateProvider authenticationStateProvider;
    private readonly PageSettings settings;

    public PageAuthoringService(
        IPageService pageService,
        IPageVersionService versionService,
        IContentBlockService contentBlockService,
        IWorkContext workContext,
        IAuthorizationService authorizationService,
        AuthenticationStateProvider authenticationStateProvider,
        PageSettings settings)
    {
        this.pageService = pageService;
        this.versionService = versionService;
        this.contentBlockService = contentBlockService;
        this.workContext = workContext;
        this.authorizationService = authorizationService;
        this.authenticationStateProvider = authenticationStateProvider;
        this.settings = settings;
    }

    #region IPageAuthoringService Members

    public async Task<PageAuthoringResult> CreatePageAsync(string name, Guid? parentId = null)
    {
        if (!await CanWriteAsync())
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotAllowed);
        }

        name = name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNameRequired);
        }

        int tenantId = workContext.CurrentTenant.Id;

        var tenantPages = ListTenantPages(tenantId);
        if (!PageHierarchy.CanBeParent(tenantPages, Guid.Empty, parentId))
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorParentInvalid);
        }

        // New pages go to the end of their level.
        int nextOrder = PageHierarchy.ChildrenOf(tenantPages, parentId).Select(sibling => sibling.Order).DefaultIfEmpty(-1).Max() + 1;

        var page = new Page
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ParentId = parentId,
            Name = name,
            Order = nextOrder,
            IsEnabled = true,
            ShowOnMenus = true,
        };
        await pageService.InsertAsync(page);

        try
        {
            return PageAuthoringResult.Success(await InsertFirstVersionAsync(page));
        }
        catch
        {
            // A page without a version cannot be shown or edited, so do not leave one behind.
            await pageService.DeleteAsync(page);
            throw;
        }
    }

    public async Task<PageAuthoringResult> SavePageAsync(Page page)
    {
        if (!await CanWriteAsync())
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotAllowed);
        }

        string name = page.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNameRequired);
        }

        var current = await pageService.FindOneAsync(page.Id);
        if (current is null)
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotFound);
        }

        if (current.TenantId != workContext.CurrentTenant.Id)
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotAllowed);
        }

        if (!PageHierarchy.CanBeParent(ListTenantPages(current.TenantId), current.Id, page.ParentId))
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorParentInvalid);
        }

        current.Name = name;
        current.Order = page.Order;
        current.ParentId = page.ParentId;
        current.IsEnabled = page.IsEnabled;
        current.ShowOnMenus = page.ShowOnMenus;
        current.AccessRestrictions = NullIfEmpty(page.AccessRestrictions);
        await pageService.UpdateAsync(current);

        return PageAuthoringResult.Success();
    }

    public async Task<PageAuthoringResult> DeletePageAsync(Guid pageId)
    {
        if (!await CanWriteAsync())
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotAllowed);
        }

        var page = await pageService.FindOneAsync(pageId);
        if (page is null)
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotFound);
        }

        if (page.TenantId != workContext.CurrentTenant.Id)
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotAllowed);
        }

        // Nothing links sub-pages to their parent in the database, so move them up first or they would be orphaned.
        foreach (var child in ListTenantPages(page.TenantId).Where(candidate => candidate.ParentId == pageId))
        {
            child.ParentId = page.ParentId;
            await pageService.UpdateAsync(child);
        }

        contentBlockService.Delete(block => block.PageId == pageId);
        await pageService.DeleteAsync(page);

        return PageAuthoringResult.Success();
    }

    public async Task<PageAuthoringResult> RestoreVersionAsync(Guid archivedVersionId)
    {
        var saved = await versionService.FindOneAsync(archivedVersionId);
        if (saved is null || saved.Status != VersionStatus.Archived)
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotFound);
        }

        if (saved.TenantId != workContext.CurrentTenant.Id)
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotAllowed);
        }

        var current = await FindCurrentVersionAsync(saved.PageId, saved.CultureCode);
        if (current is null)
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotFound);
        }

        return await ApplyAsync(current.Id, edited =>
        {
            edited.Title = saved.Title;
            edited.Fields = saved.Fields;

            // The address may have been taken by another page since the save point was made.
            if (!SlugInUse(saved.TenantId, saved.CultureCode, saved.Slug, saved.PageId))
            {
                edited.Slug = saved.Slug;
            }

            return null;
        });
    }

    public async Task<PageAuthoringResult> EnsureFirstVersionAsync(Guid pageId)
    {
        var existing = await FindCurrentVersionAsync(pageId, null);
        if (existing is not null)
        {
            return PageAuthoringResult.Success(existing);
        }

        if (!await CanWriteAsync())
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotAllowed);
        }

        var page = await pageService.FindOneAsync(pageId);
        if (page is null || page.TenantId != workContext.CurrentTenant.Id)
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotFound);
        }

        return PageAuthoringResult.Success(await InsertFirstVersionAsync(page));
    }

    private async Task<PageVersion> InsertFirstVersionAsync(Page page)
    {
        var now = DateTime.UtcNow;
        return await versionService.InsertAsync(new PageVersion
        {
            Id = Guid.NewGuid(),
            TenantId = page.TenantId,
            PageId = page.Id,
            CultureCode = null,
            Status = VersionStatus.Draft,
            Title = page.Name,
            Slug = FindFreeSlug(page.TenantId, null, Slugify(page.Name), page.Id),
            DateCreatedUtc = now,
            DateModifiedUtc = now,
        });
    }

    public async Task<PageVersion> FindCurrentVersionAsync(Guid pageId, string cultureCode = null)
    {
        cultureCode = NormalizeCulture(cultureCode);

        List<PageVersion> candidates;
        using (var connection = versionService.OpenConnection())
        {
            candidates = connection
                .Query(version => version.PageId == pageId && version.CultureCode == cultureCode && version.Status != VersionStatus.Archived)
                .ToList();
        }

        return await Task.FromResult(candidates
            .OrderByDescending(version => version.Status == VersionStatus.Published)
            .ThenByDescending(version => version.DateModifiedUtc)
            .FirstOrDefault());
    }

    public async Task<IReadOnlyList<PageVersion>> ListHistoryAsync(Guid pageId, string cultureCode = null)
    {
        cultureCode = NormalizeCulture(cultureCode);

        List<PageVersion> history;
        using (var connection = versionService.OpenConnection())
        {
            history = connection
                .Query(version => version.PageId == pageId && version.CultureCode == cultureCode && version.Status == VersionStatus.Archived)
                .OrderByDescending(version => version.DateModifiedUtc)
                .ToList();
        }

        return await Task.FromResult(history);
    }

    public async Task<PageAuthoringResult> CreateTranslationAsync(Guid pageId, string cultureCode)
    {
        if (!await CanWriteAsync())
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotAllowed);
        }

        cultureCode = NormalizeCulture(cultureCode);
        if (cultureCode is null)
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorTranslationExists);
        }

        if (await FindCurrentVersionAsync(pageId, cultureCode) is not null)
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorTranslationExists);
        }

        var source = await FindCurrentVersionAsync(pageId, null);
        if (source is null || source.TenantId != workContext.CurrentTenant.Id)
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotFound);
        }

        var now = DateTime.UtcNow;
        var version = await versionService.InsertAsync(new PageVersion
        {
            Id = Guid.NewGuid(),
            TenantId = source.TenantId,
            PageId = pageId,
            CultureCode = cultureCode,
            Status = VersionStatus.Draft,
            Title = source.Title,
            Slug = FindFreeSlug(source.TenantId, cultureCode, source.Slug, pageId),
            Fields = source.Fields,
            DateCreatedUtc = now,
            DateModifiedUtc = now,
        });

        return PageAuthoringResult.Success(version);
    }

    public Task<PageAuthoringResult> SaveDetailsAsync(PageVersionDetails details) =>
        ApplyAsync(details.VersionId, edited =>
        {
            string title = details.Title?.Trim();
            if (string.IsNullOrEmpty(title))
            {
                return InfernoCmsLocalizableStrings.Pages.ErrorTitleRequired;
            }

            string slug = details.Slug?.Trim().Trim('/');
            if (string.IsNullOrEmpty(slug))
            {
                return InfernoCmsLocalizableStrings.Pages.ErrorSlugRequired;
            }

            edited.Title = title;
            edited.Slug = slug;
            edited.Status = details.Published ? VersionStatus.Published : VersionStatus.Draft;

            var fields = StandardPageContent.Parse(edited.Fields);
            string metaTitle = NullIfEmpty(details.MetaTitle);
            string metaKeywords = NullIfEmpty(details.MetaKeywords);
            string metaDescription = NullIfEmpty(details.MetaDescription);

            // Only rewrite the stored JSON when a value really changed, so pressing Save on an
            // untouched page does not count as an edit.
            if (NullIfEmpty(fields.MetaTitle) != metaTitle ||
                NullIfEmpty(fields.MetaKeywords) != metaKeywords ||
                NullIfEmpty(fields.MetaDescription) != metaDescription)
            {
                fields.MetaTitle = metaTitle;
                fields.MetaKeywords = metaKeywords;
                fields.MetaDescription = metaDescription;
                edited.Fields = StandardPageContent.Serialize(fields);
            }

            return null;
        });

    public Task<PageAuthoringResult> SetPublishedAsync(Guid versionId, bool published) =>
        ApplyAsync(versionId, edited =>
        {
            edited.Status = published ? VersionStatus.Published : VersionStatus.Draft;
            return null;
        });

    public Task<PageAuthoringResult> SaveContentAsync(Guid versionId, string documentJson, string html) =>
        ApplyAsync(versionId, edited =>
        {
            var fields = StandardPageContent.Parse(edited.Fields);
            fields.Document = documentJson;
            fields.BodyContent = html;
            edited.Fields = StandardPageContent.Serialize(fields);
            return null;
        });

    #endregion IPageAuthoringService Members

    /// <summary>
    /// Loads the version, lets <paramref name="mutate"/> change a copy of it, and saves if anything changed.
    /// When its title, address or content change, the previous state is first kept as an archived save point.
    /// </summary>
    /// <param name="mutate">Changes the copy. Returns an error key to refuse the change, or null.</param>
    private async Task<PageAuthoringResult> ApplyAsync(Guid versionId, Func<PageVersion, string> mutate)
    {
        if (!await CanWriteAsync())
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotAllowed);
        }

        var current = await versionService.FindOneAsync(versionId);
        if (current is null || current.Status == VersionStatus.Archived)
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotFound);
        }

        if (current.TenantId != workContext.CurrentTenant.Id)
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorNotAllowed);
        }

        var edited = new PageVersion
        {
            Status = current.Status,
            Title = current.Title,
            Slug = current.Slug,
            Fields = current.Fields,
        };

        string error = mutate(edited);
        if (error is not null)
        {
            return PageAuthoringResult.Failure(error);
        }

        edited.Fields = MediaHelper.EnsureCorrectUrls(edited.Fields);

        bool unchanged =
            edited.Status == current.Status &&
            edited.Title == current.Title &&
            edited.Slug == current.Slug &&
            edited.Fields == current.Fields;

        if (unchanged)
        {
            return PageAuthoringResult.Success(current);
        }

        if (edited.Slug != current.Slug && SlugInUse(current.TenantId, current.CultureCode, edited.Slug, current.PageId))
        {
            return PageAuthoringResult.Failure(InfernoCmsLocalizableStrings.Pages.ErrorSlugInUse);
        }

        bool archive = ShouldKeepSavePoint(current, edited);

        if (archive)
        {
            await versionService.InsertAsync(new PageVersion
            {
                Id = Guid.NewGuid(),
                TenantId = current.TenantId,
                PageId = current.PageId,
                CultureCode = current.CultureCode,
                DateCreatedUtc = current.DateCreatedUtc,
                DateModifiedUtc = current.DateModifiedUtc,
                Status = VersionStatus.Archived,
                Title = current.Title,
                Slug = current.Slug,
                Fields = current.Fields,
            });
        }

        current.Status = edited.Status;
        current.Title = edited.Title;
        current.Slug = edited.Slug;
        current.Fields = edited.Fields;
        current.DateModifiedUtc = DateTime.UtcNow;
        await versionService.UpdateAsync(current);

        if (archive)
        {
            PruneHistory(current.PageId, current.CultureCode);
        }

        return PageAuthoringResult.Success(current);
    }

    /// <summary>
    /// Whether replacing <paramref name="current"/> with <paramref name="edited"/> should keep the earlier state as a
    /// save point. Every save that changes what the page says does, whether the page is a draft or published and
    /// however soon after the last save it comes. Switching between draft and published changes nothing a visitor
    /// reads, and a page that has never held anything has nothing worth restoring.
    /// </summary>
    internal static bool ShouldKeepSavePoint(PageVersion current, PageVersion edited)
    {
        bool contentChanged =
            edited.Title != current.Title ||
            edited.Slug != current.Slug ||
            edited.Fields != current.Fields;

        return contentChanged && !string.IsNullOrWhiteSpace(current.Fields);
    }

    /// <summary>Keeps the newest save points, up to the configured number, and removes the rest. The version being edited is never touched.</summary>
    private void PruneHistory(Guid pageId, string cultureCode)
    {
        int keep = Math.Max(1, (int)settings.NumberOfPageVersionsToKeep);

        List<Guid> idsToKeep;
        using (var connection = versionService.OpenConnection())
        {
            idsToKeep = connection
                .Query(version => version.PageId == pageId && version.CultureCode == cultureCode && version.Status == VersionStatus.Archived)
                .OrderByDescending(version => version.DateModifiedUtc)
                .Take(keep)
                .Select(version => version.Id)
                .ToList();
        }

        versionService.Delete(version =>
            version.PageId == pageId &&
            version.CultureCode == cultureCode &&
            version.Status == VersionStatus.Archived &&
            !idsToKeep.Contains(version.Id));
    }

    private List<Page> ListTenantPages(int? tenantId)
    {
        using var connection = pageService.OpenConnection();
        return connection.Query(page => page.TenantId == tenantId).ToList();
    }

    private async Task<bool> CanWriteAsync()
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        return (await authorizationService.AuthorizeAsync(state.User, CmsConstants.Policies.PagesWrite)).Succeeded;
    }

    private bool SlugInUse(int? tenantId, string cultureCode, string slug, Guid exceptPageId)
    {
        using var connection = versionService.OpenConnection();
        return connection
            .Query(version =>
                version.TenantId == tenantId &&
                version.CultureCode == cultureCode &&
                version.Status != VersionStatus.Archived &&
                version.PageId != exceptPageId &&
                version.Slug == slug)
            .Any();
    }

    private string FindFreeSlug(int? tenantId, string cultureCode, string baseSlug, Guid exceptPageId)
    {
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = "page";
        }

        string slug = baseSlug;
        for (int suffix = 2; SlugInUse(tenantId, cultureCode, slug, exceptPageId); suffix++)
        {
            slug = $"{baseSlug}-{suffix}";
        }

        return slug;
    }

    private static string NormalizeCulture(string cultureCode) =>
        string.IsNullOrWhiteSpace(cultureCode) ? null : cultureCode.Trim();

    private static string NullIfEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    internal static string Slugify(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : Regex.Replace(value.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
}
