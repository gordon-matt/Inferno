using System.Security.Claims;
using Inferno.Web;
using Inferno.Web.ContentManagement.Areas.Admin.Blog;
using Microsoft.EntityFrameworkCore;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Inferno.Web.ContentManagement.Infrastructure;
using Microsoft.Extensions.Localization;

namespace Inferno.Web.ContentManagement.Components;

public class CmsMenuBuilder
{
    private readonly IPageVersionService pageVersionService;
    private readonly IEnumerable<IAutoMenuProvider> menuProviders;
    private readonly BlogSettings blogSettings;
    private readonly IWorkContext workContext;
    private readonly IStringLocalizer localizer;

    public CmsMenuBuilder(
        IPageVersionService pageVersionService,
        IEnumerable<IAutoMenuProvider> menuProviders,
        BlogSettings blogSettings,
        IWorkContext workContext,
        IStringLocalizer localizer)
    {
        this.pageVersionService = pageVersionService;
        this.menuProviders = menuProviders;
        this.blogSettings = blogSettings;
        this.workContext = workContext;
        this.localizer = localizer;
    }

    public async Task<IReadOnlyList<MenuItem>> BuildMainMenuAsync(ClaimsPrincipal user, bool includeHomePageLink)
    {
        var menuItems = new List<MenuItem>();
        int tenantId = workContext.CurrentTenant.Id;

        if (includeHomePageLink)
        {
            menuItems.Add(new MenuItem
            {
                Id = Guid.NewGuid(),
                Text = localizer[InfernoWebLocalizableStrings.General.Home],
                Url = "/",
                Enabled = true,
                Position = -1
            });
        }

        var pageVersions = pageVersionService.GetCurrentVersions(
            tenantId,
            workContext.CurrentCultureCode,
            enabledOnly: true,
            shownOnMenusOnly: true);

        foreach (var version in pageVersions)
        {
            if (!await PageSecurityHelper.CheckUserHasAccessToPage(version.Page, user))
            {
                continue;
            }

            menuItems.Add(new MenuItem
            {
                Id = version.Page.Id,
                Text = version.Title,
                Url = "/" + version.Slug,
                Enabled = true,
                ParentId = version.Page.ParentId,
                Position = version.Page.Order
            });
        }

        if (blogSettings.ShowOnMenus && await PageSecurityHelper.CheckUserHasAccessToBlog(user))
        {
            menuItems.Add(new MenuItem
            {
                Id = Guid.NewGuid(),
                Text = blogSettings.PageTitle,
                Url = "/blog",
                Enabled = true,
                Position = blogSettings.MenuPosition
            });
        }

        foreach (var provider in menuProviders)
        {
            menuItems.AddRange(provider.GetMainMenuItems(user));
        }

        return menuItems
            .OrderBy(x => x.Position)
            .ThenBy(x => x.Text)
            .ToList();
    }

    public async Task<IReadOnlyList<MenuItem>> BuildSubMenuAsync(ClaimsPrincipal user, string currentUrlSlug)
    {
        var menuItems = new List<MenuItem>();
        int tenantId = workContext.CurrentTenant.Id;
        currentUrlSlug ??= string.Empty;

        if (string.IsNullOrEmpty(currentUrlSlug))
        {
            if (blogSettings.ShowOnMenus && await PageSecurityHelper.CheckUserHasAccessToBlog(user))
            {
                menuItems.Add(new MenuItem
                {
                    Id = Guid.NewGuid(),
                    Text = blogSettings.PageTitle,
                    Url = "/blog",
                    Enabled = true,
                    Position = blogSettings.MenuPosition
                });
            }

            foreach (var provider in menuProviders)
            {
                menuItems.AddRange(provider.GetMainMenuItems(user).Where(x => x.ParentId == null));
            }
        }

        foreach (var provider in menuProviders)
        {
            if (!string.IsNullOrEmpty(provider.RootUrlSlug) && currentUrlSlug.StartsWith(provider.RootUrlSlug, StringComparison.OrdinalIgnoreCase))
            {
                menuItems.AddRange(provider.GetSubMenuItems(currentUrlSlug, user));
            }
        }

        var pageVersions = Enumerable.Empty<Areas.Admin.Pages.Entities.PageVersion>();
        bool hasCmsPages = true;

        if (string.IsNullOrEmpty(currentUrlSlug))
        {
            pageVersions = pageVersionService.GetCurrentVersions(
                tenantId,
                workContext.CurrentCultureCode,
                enabledOnly: true,
                shownOnMenusOnly: true,
                topLevelOnly: true);
        }
        else
        {
            Areas.Admin.Pages.Entities.PageVersion anyVersion;
            using (var connection = pageVersionService.OpenConnection())
            {
                anyVersion = connection.Query(x => x.TenantId == tenantId && x.Slug == currentUrlSlug)
                    .Include(x => x.Page)
                    .FirstOrDefault();
            }

            if (anyVersion != null)
            {
                pageVersions = pageVersionService.GetCurrentVersions(
                    tenantId,
                    workContext.CurrentCultureCode,
                    enabledOnly: true,
                    shownOnMenusOnly: true,
                    parentId: anyVersion.Page.Id);
            }
            else
            {
                hasCmsPages = false;
            }
        }

        if (hasCmsPages)
        {
            foreach (var version in pageVersions)
            {
                if (!await PageSecurityHelper.CheckUserHasAccessToPage(version.Page, user))
                {
                    continue;
                }

                menuItems.Add(new MenuItem
                {
                    Id = version.Page.Id,
                    Text = version.Title,
                    Url = "/" + version.Slug,
                    Enabled = true,
                    ParentId = version.Page.ParentId,
                    Position = version.Page.Order
                });
            }
        }

        return menuItems
            .OrderBy(x => x.Position)
            .ThenBy(x => x.Text)
            .ToList();
    }
}
