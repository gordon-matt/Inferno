using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;

/// <summary>Loads pages for a tenant.</summary>
public interface IPageService : IGenericDataService<Page>
{
    /// <summary>Loads a tenant's pages that have no parent.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <returns>The top-level pages.</returns>
    IEnumerable<Page> GetTopLevelPages(int tenantId);
}

/// <inheritdoc cref="IPageService" />
public class PageService : GenericDataService<Page>, IPageService
{
    /// <summary>Passes the cache and the page repository to the generic data service.</summary>
    /// <param name="cacheManager">The cache used by the unfiltered and primary-key finds.</param>
    /// <param name="repository">The page store.</param>
    public PageService(ICacheManager cacheManager, IRepository<Page> repository)
        : base(cacheManager, repository)
    {
    }

    /// <inheritdoc />
    public IEnumerable<Page> GetTopLevelPages(int tenantId)
    {
        using var connection = OpenConnection();
        return connection
            .Query(x => x.TenantId == tenantId && x.ParentId == null)
            .OrderBy(x => x.Name)
            .ToHashSet();
    }
}
