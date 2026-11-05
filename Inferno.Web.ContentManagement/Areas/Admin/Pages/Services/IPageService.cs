using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;

public interface IPageService : IGenericDataService<Page>
{
    IEnumerable<Page> GetTopLevelPages(int tenantId);
}

public class PageService : GenericDataService<Page>, IPageService
{
    public PageService(ICacheManager cacheManager, IRepository<Page> repository)
        : base(cacheManager, repository)
    {
    }

    public IEnumerable<Page> GetTopLevelPages(int tenantId)
    {
        using var connection = OpenConnection();
        return connection
            .Query(x => x.TenantId == tenantId && x.ParentId == null)
            .OrderBy(x => x.Name)
            .ToHashSet();
    }
}
