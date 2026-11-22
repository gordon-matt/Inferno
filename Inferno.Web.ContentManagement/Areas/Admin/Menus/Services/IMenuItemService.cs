using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Menus.Services;

/// <summary>Loads the items of a menu.</summary>
public interface IMenuItemService : IGenericDataService<MenuItem>
{
    /// <summary>Finds the menu item that points at a record.</summary>
    /// <param name="refId">The referenced record. <see cref="Guid.Empty"/> matches nothing.</param>
    /// <returns>The menu item, or <see langword="null"/> when <paramref name="refId"/> is empty or no item points at it.</returns>
    MenuItem GetMenuItemByRefId(Guid refId);

    /// <summary>Loads a menu's items, ordered by position and then text.</summary>
    /// <param name="menuId">The menu.</param>
    /// <param name="enabledOnly"><see langword="true"/> to omit disabled items.</param>
    /// <returns>The items. The list is cached until this service changes menu items.</returns>
    IEnumerable<MenuItem> GetMenuItems(Guid menuId, bool enabledOnly = false);
}

/// <inheritdoc cref="IMenuItemService" />
public class MenuItemService : GenericDataService<MenuItem>, IMenuItemService
{
    /// <summary>Passes the cache and the menu-item repository to the generic data service.</summary>
    /// <param name="cacheManager">The cache used by the unfiltered and primary-key finds, and by <see cref="IMenuItemService.GetMenuItems"/>.</param>
    /// <param name="repository">The menu-item store.</param>
    public MenuItemService(ICacheManager cacheManager, IRepository<MenuItem> repository)
        : base(cacheManager, repository)
    {
    }

    /// <inheritdoc />
    public MenuItem GetMenuItemByRefId(Guid refId) => refId == Guid.Empty
            ? null
            : FindOne(new SearchOptions<MenuItem>
            {
                Query = x => x.RefId == refId
            });

    /// <inheritdoc />
    public IEnumerable<MenuItem> GetMenuItems(Guid menuId, bool enabledOnly = false) =>
        CacheManager.Get(string.Format("Repository_MenuItem_GetByMenuIdAndEnabled_{0}_{1}", menuId, enabledOnly), () =>
        {
            using var connection = OpenConnection();
            return enabledOnly
                ? connection
                    .Query(x => x.MenuId == menuId && x.Enabled)
                    .OrderBy(x => x.Position)
                    .ThenBy(x => x.Text)
                    .ToList()
                : connection
                    .Query(x => x.MenuId == menuId)
                    .OrderBy(x => x.Position)
                    .ThenBy(x => x.Text)
                    .ToList();
        });

    protected override void ClearCache()
    {
        base.ClearCache();
        CacheManager.RemoveByPattern("Repository_MenuItem_GetByMenuIdAndEnabled_.*");
    }
}