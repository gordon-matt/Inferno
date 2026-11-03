using Ardalis.Result;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.ContentManagement.Areas.Admin.Menus.Services;

public sealed record MenuSummary(Guid Id, string Name, string UrlFilter, int ItemCount);

public interface IMenuAdminService
{
    Task<Result<IReadOnlyList<MenuSummary>>> ListMenusAsync(int tenantId, string search);

    Task<Result<Menu>> GetMenuAsync(int tenantId, Guid id);

    /// <summary>Inserts when <see cref="Menu.Id"/> is empty, otherwise updates.</summary>
    Task<Result<Menu>> SaveMenuAsync(int tenantId, Menu menu);

    /// <summary>Removes the menu together with all of its items.</summary>
    Task<Result> DeleteMenuAsync(int tenantId, Guid id);

    Task<Result<IReadOnlyList<MenuItem>>> ListItemsAsync(int tenantId, Guid menuId);

    /// <summary>Inserts when <see cref="MenuItem.Id"/> is empty, otherwise updates. The parent must belong to the same menu.</summary>
    Task<Result<MenuItem>> SaveItemAsync(int tenantId, MenuItem item);

    /// <summary>Removes the item and everything nested under it.</summary>
    Task<Result> DeleteItemAsync(int tenantId, Guid id);
}

public class MenuAdminService(
    IMenuService menuService,
    IMenuItemService menuItemService,
    ILogger<MenuAdminService> logger) : IMenuAdminService
{
    public Task<Result<IReadOnlyList<MenuSummary>>> ListMenusAsync(int tenantId, string search)
    {
        using var menuConnection = menuService.OpenConnection();
        var query = menuConnection.Query(menu => menu.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(menu => menu.Name.Contains(term) || menu.UrlFilter.Contains(term));
        }

        var menus = query.OrderBy(menu => menu.Name).ToList();
        var menuIds = menus.Select(menu => menu.Id).ToList();

        using var itemConnection = menuItemService.OpenConnection();
        var counts = itemConnection.Query(item => menuIds.Contains(item.MenuId))
            .GroupBy(item => item.MenuId)
            .Select(group => new { MenuId = group.Key, Count = group.Count() })
            .ToDictionary(x => x.MenuId, x => x.Count);

        IReadOnlyList<MenuSummary> summaries = menus
            .Select(menu => new MenuSummary(menu.Id, menu.Name, menu.UrlFilter, counts.GetValueOrDefault(menu.Id)))
            .ToList();

        return Task.FromResult(Result.Success(summaries));
    }

    public async Task<Result<Menu>> GetMenuAsync(int tenantId, Guid id)
    {
        var menu = await FindMenuAsync(tenantId, id);
        return menu is null ? Result<Menu>.NotFound() : Result.Success(menu);
    }

    public async Task<Result<Menu>> SaveMenuAsync(int tenantId, Menu menu)
    {
        if (string.IsNullOrWhiteSpace(menu.Name))
        {
            return Result<Menu>.Invalid(new ValidationError { Identifier = nameof(Menu.Name), ErrorMessage = "Enter a name." });
        }

        try
        {
            if (menu.Id != Guid.Empty)
            {
                var existing = await FindMenuAsync(tenantId, menu.Id);
                if (existing is null)
                {
                    return Result<Menu>.NotFound();
                }

                existing.Name = menu.Name.Trim();
                existing.UrlFilter = menu.UrlFilter?.Trim();
                await menuService.UpdateAsync(existing);
                return Result.Success(existing);
            }

            var created = await menuService.InsertAsync(new Menu
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = menu.Name.Trim(),
                UrlFilter = menu.UrlFilter?.Trim()
            });

            return Result.Success(created);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not save menu {MenuId} for tenant {TenantId}", menu.Id, tenantId);
            return Result<Menu>.Error("The menu could not be saved.");
        }
    }

    public async Task<Result> DeleteMenuAsync(int tenantId, Guid id)
    {
        var menu = await FindMenuAsync(tenantId, id);
        if (menu is null)
        {
            return Result.NotFound();
        }

        try
        {
            await menuItemService.DeleteAsync(item => item.MenuId == id);
            await menuService.DeleteAsync(menu);
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not delete menu {MenuId} for tenant {TenantId}", id, tenantId);
            return Result.Error("The menu could not be deleted.");
        }
    }

    public async Task<Result<IReadOnlyList<MenuItem>>> ListItemsAsync(int tenantId, Guid menuId)
    {
        if (await FindMenuAsync(tenantId, menuId) is null)
        {
            return Result<IReadOnlyList<MenuItem>>.NotFound();
        }

        IReadOnlyList<MenuItem> items = menuItemService.GetMenuItems(menuId).ToList();
        return Result.Success(items);
    }

    public async Task<Result<MenuItem>> SaveItemAsync(int tenantId, MenuItem item)
    {
        if (await FindMenuAsync(tenantId, item.MenuId) is null)
        {
            return Result<MenuItem>.NotFound();
        }

        var errors = new List<ValidationError>();
        if (string.IsNullOrWhiteSpace(item.Text))
        {
            errors.Add(new ValidationError { Identifier = nameof(MenuItem.Text), ErrorMessage = "Enter the link text." });
        }

        if (string.IsNullOrWhiteSpace(item.Url))
        {
            errors.Add(new ValidationError { Identifier = nameof(MenuItem.Url), ErrorMessage = "Enter a URL." });
        }

        var siblings = menuItemService.GetMenuItems(item.MenuId).ToList();
        if (item.ParentId.HasValue && !IsValidParent(siblings, item.Id, item.ParentId.Value))
        {
            errors.Add(new ValidationError { Identifier = nameof(MenuItem.ParentId), ErrorMessage = "Choose a parent that is not this item or one of its children." });
        }

        if (errors.Count > 0)
        {
            return Result<MenuItem>.Invalid(errors);
        }

        try
        {
            if (item.Id != Guid.Empty)
            {
                var existing = await menuItemService.FindOneAsync(item.Id);
                if (existing is null || existing.MenuId != item.MenuId)
                {
                    return Result<MenuItem>.NotFound();
                }

                Copy(item, existing);
                await menuItemService.UpdateAsync(existing);
                return Result.Success(existing);
            }

            var created = new MenuItem { Id = Guid.NewGuid(), MenuId = item.MenuId };
            Copy(item, created);
            await menuItemService.InsertAsync(created);
            return Result.Success(created);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not save menu item {MenuItemId} in menu {MenuId}", item.Id, item.MenuId);
            return Result<MenuItem>.Error("The menu item could not be saved.");
        }
    }

    public async Task<Result> DeleteItemAsync(int tenantId, Guid id)
    {
        var item = await menuItemService.FindOneAsync(id);
        if (item is null || await FindMenuAsync(tenantId, item.MenuId) is null)
        {
            return Result.NotFound();
        }

        try
        {
            var all = menuItemService.GetMenuItems(item.MenuId).ToList();
            var doomed = new HashSet<Guid> { id };
            bool added;
            do
            {
                added = false;
                foreach (var child in all.Where(x => x.ParentId.HasValue && doomed.Contains(x.ParentId.Value) && !doomed.Contains(x.Id)))
                {
                    doomed.Add(child.Id);
                    added = true;
                }
            }
            while (added);

            var doomedIds = doomed.ToList();
            await menuItemService.DeleteAsync(x => doomedIds.Contains(x.Id));
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not delete menu item {MenuItemId}", id);
            return Result.Error("The menu item could not be deleted.");
        }
    }

    private async Task<Menu> FindMenuAsync(int tenantId, Guid id)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var menu = await menuService.FindOneAsync(id);
        return menu?.TenantId == tenantId ? menu : null;
    }

    // Walks up from the proposed parent; reaching the item itself would create a loop.
    private static bool IsValidParent(IReadOnlyList<MenuItem> items, Guid itemId, Guid parentId)
    {
        var byId = items.ToDictionary(x => x.Id);
        Guid? current = parentId;
        var seen = new HashSet<Guid>();
        while (current.HasValue)
        {
            if (current.Value == itemId || !seen.Add(current.Value) || !byId.TryGetValue(current.Value, out var parent))
            {
                return false;
            }

            current = parent.ParentId;
        }

        return true;
    }

    private static void Copy(MenuItem source, MenuItem target)
    {
        target.Text = source.Text.Trim();
        target.Url = source.Url.Trim();
        target.Description = source.Description?.Trim();
        target.CssClass = source.CssClass?.Trim();
        target.Position = source.Position;
        target.ParentId = source.ParentId;
        target.Enabled = source.Enabled;
        target.IsExternalUrl = source.IsExternalUrl;
        target.RefId = source.RefId;
    }
}
