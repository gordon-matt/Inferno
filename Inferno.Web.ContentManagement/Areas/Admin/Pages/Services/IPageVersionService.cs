using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;

/// <summary>Chooses the current draft or published version of a page.</summary>
public interface IPageVersionService : IGenericDataService<PageVersion>
{
    /// <summary>
    /// Gets the current version of one page: the newest published version for <paramref name="cultureCode"/>,
    /// then its newest draft, then the same two steps for the invariant culture. Archived versions are skipped.
    /// </summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="pageId">The page.</param>
    /// <param name="cultureCode">The culture to try first. Empty or <see langword="null"/> uses only the invariant culture.</param>
    /// <param name="enabledOnly"><see langword="true"/> to require the page to be enabled.</param>
    /// <param name="shownOnMenusOnly"><see langword="true"/> to require the page to be shown on menus.</param>
    /// <returns>The chosen version, or <see langword="null"/> when none matches.</returns>
    PageVersion GetCurrentVersion(
        int tenantId,
        Guid pageId,
        string cultureCode = null,
        bool enabledOnly = true,
        bool shownOnMenusOnly = true);

    /// <summary>Gets <see cref="GetCurrentVersion"/> for each page that matches the filters.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="cultureCode">The culture to try first. Empty or <see langword="null"/> uses only the invariant culture.</param>
    /// <param name="enabledOnly"><see langword="true"/> to include only enabled pages.</param>
    /// <param name="shownOnMenusOnly"><see langword="true"/> to include only pages shown on menus.</param>
    /// <param name="topLevelOnly"><see langword="true"/> to include only pages with no parent. When set, <paramref name="parentId"/> is ignored.</param>
    /// <param name="parentId">When <paramref name="topLevelOnly"/> is <see langword="false"/> and this is set, includes only children of this page.</param>
    /// <returns>One current version per matching page. A page with no current version is omitted.</returns>
    IEnumerable<PageVersion> GetCurrentVersions(
        int tenantId,
        string cultureCode = null,
        bool enabledOnly = true,
        bool shownOnMenusOnly = true,
        bool topLevelOnly = false,
        Guid? parentId = null);
}

/// <inheritdoc cref="IPageVersionService" />
public class PageVersionService : GenericDataService<PageVersion>, IPageVersionService
{
    private readonly IRepository<Page> pageRepository;

    /// <summary>Stores the cache, the version repository, and the page repository.</summary>
    /// <param name="cacheManager">The cache used by the generic data service.</param>
    /// <param name="repository">The page-version store.</param>
    /// <param name="pageRepository">Loads pages when listing current versions.</param>
    public PageVersionService(
        ICacheManager cacheManager,
        IRepository<PageVersion> repository,
        IRepository<Page> pageRepository)
        : base(cacheManager, repository)
    {
        this.pageRepository = pageRepository;
    }

    #region IPageVersionService Members

    /// <inheritdoc />
    public PageVersion GetCurrentVersion(
        int tenantId,
        Guid pageId,
        string cultureCode = null,
        bool enabledOnly = true,
        bool shownOnMenusOnly = true)
    {
        using var pageVersionConnection = OpenConnection();
        IQueryable<PageVersion> query = pageVersionConnection.Query(x => x.TenantId == tenantId).Include(x => x.Page);

        if (enabledOnly)
        {
            query = query.Where(x => x.Page.IsEnabled);
        }

        if (shownOnMenusOnly)
        {
            query = query.Where(x => x.Page.ShowOnMenus);
        }

        return GetCurrentVersionInternal(pageId, query, cultureCode);
    }

    /// <inheritdoc />
    public IEnumerable<PageVersion> GetCurrentVersions(
        int tenantId,
        string cultureCode = null,
        bool enabledOnly = true,
        bool shownOnMenusOnly = true,
        bool topLevelOnly = false,
        Guid? parentId = null)
    {
        ICollection<Page> pages = null;

        using (var pageConnection = pageRepository.OpenConnection())
        {
            var query = pageConnection.Query(x => x.TenantId == tenantId);

            if (enabledOnly)
            {
                query = query.Where(x => x.IsEnabled);
            }

            if (shownOnMenusOnly)
            {
                query = query.Where(x => x.ShowOnMenus);
            }

            if (topLevelOnly)
            {
                query = query.Where(x => x.ParentId == null);
            }
            else if (parentId.HasValue)
            {
                query = query.Where(x => x.ParentId == parentId);
            }

            pages = query.ToHashSet();
        }

        using var pageVersionConnection = OpenConnection();
        var pageVersions = pageVersionConnection
            .Query(x => x.TenantId == tenantId)
            .Include(x => x.Page)
            .ToHashSet();

        return pages
            .Select(x => GetCurrentVersionInternal(x.Id, pageVersions, cultureCode))
            .Where(x => x != null);
    }

    #endregion IPageVersionService Members

    private static PageVersion GetCurrentVersionInternal(
        Guid pageId,
        IEnumerable<PageVersion> pageVersions,
        string cultureCode = null)
    {
        if (!string.IsNullOrEmpty(cultureCode))
        {
            var localizedVersions = pageVersions
                .Where(x =>
                    x.PageId == pageId &&
                    x.CultureCode == cultureCode &&
                    x.Status != VersionStatus.Archived);

            var localizedVersion = localizedVersions
                    .Where(x => x.Status == VersionStatus.Published)
                    .OrderByDescending(x => x.DateModifiedUtc)
                    .FirstOrDefault();

            localizedVersion ??= localizedVersions
                    .Where(x => x.Status == VersionStatus.Draft)
                    .OrderByDescending(x => x.DateModifiedUtc)
                    .FirstOrDefault();

            if (localizedVersion != null)
            {
                return localizedVersion;
            }
        }

        var invariantVersions = pageVersions
            .Where(x =>
                x.PageId == pageId &&
                x.CultureCode == null &&
                x.Status != VersionStatus.Archived);

        var publishedVersion = invariantVersions
            .Where(x => x.Status == VersionStatus.Published)
            .OrderByDescending(x => x.DateModifiedUtc)
            .FirstOrDefault();

        return publishedVersion ?? invariantVersions
                .Where(x => x.Status == VersionStatus.Draft)
                .OrderByDescending(x => x.DateModifiedUtc)
                .FirstOrDefault();
    }
}