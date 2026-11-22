using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Menus.Services;

/// <summary>Loads menus for a tenant.</summary>
public interface IMenuService : IGenericDataService<Menu>
{
    /// <summary>Finds a tenant's menu by name.</summary>
    /// <param name="tenantId">The tenant that owns the menu.</param>
    /// <param name="name">The menu name.</param>
    /// <param name="urlFilter">When empty, matches a menu whose URL filter is empty. Otherwise matches a menu whose URL filter contains this text.</param>
    /// <returns>The menu, or <see langword="null"/> when none matches.</returns>
    Menu FindByName(int tenantId, string name, string urlFilter = null);
}

/// <inheritdoc cref="IMenuService" />
public class MenuService : GenericDataService<Menu>, IMenuService
{
    /// <summary>Passes the cache and the menu repository to the generic data service.</summary>
    /// <param name="cacheManager">The cache used by the unfiltered and primary-key finds.</param>
    /// <param name="repository">The menu store.</param>
    public MenuService(ICacheManager cacheManager, IRepository<Menu> repository)
        : base(cacheManager, repository)
    {
    }

    #region IMenuService Members

    /// <inheritdoc />
    public Menu FindByName(int tenantId, string name, string urlFilter = null) => string.IsNullOrEmpty(urlFilter)
            ? FindOne(new SearchOptions<Menu>
            {
                Query = x =>
                    x.TenantId == tenantId
                    && x.Name == name
                    && (x.UrlFilter == null || x.UrlFilter == "")
            })
            : FindOne(new SearchOptions<Menu>
            {
                Query = x =>
                    x.TenantId == tenantId
                    && x.Name == name
                    && x.UrlFilter.Contains(urlFilter)
            });

    #endregion IMenuService Members
}