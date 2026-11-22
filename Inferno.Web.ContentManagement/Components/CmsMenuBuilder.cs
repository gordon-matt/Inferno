using System.Security.Claims;
using Inferno.Web;
using Inferno.Web.Configuration;
using Microsoft.EntityFrameworkCore;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Inferno.Web.ContentManagement.Infrastructure;
using Microsoft.Extensions.Localization;

namespace Inferno.Web.ContentManagement.Components;

/// <summary>Builds the public main menu and the submenu for the current page.</summary>
public class CmsMenuBuilder
{
    private readonly IPageVersionService pageVersionService;
    private readonly IEnumerable<IAutoMenuProvider> menuProviders;
    private readonly IWorkContext workContext;
    private readonly IStringLocalizer localizer;
    private readonly SiteSettings siteSettings;

    /// <summary>Stores the page, menu-provider, tenant, localization, and site settings used to build menus.</summary>
    /// <param name="pageVersionService">Loads the current page versions.</param>
    /// <param name="menuProviders">Extra items added after the pages.</param>
    /// <param name="workContext">Supplies the current tenant and culture.</param>
    /// <param name="localizer">Supplies the Home and Blog captions.</param>
    /// <param name="siteSettings">Decides whether the blog link is included.</param>
    public CmsMenuBuilder(
        IPageVersionService pageVersionService,
        IEnumerable<IAutoMenuProvider> menuProviders,
        IWorkContext workContext,
        IStringLocalizer localizer,
        SiteSettings siteSettings)
    {
        this.pageVersionService = pageVersionService;
        this.menuProviders = menuProviders;
        this.workContext = workContext;
        this.localizer = localizer;
        this.siteSettings = siteSettings;
    }

    /// <summary>
    /// Builds the main menu: an optional home link, enabled pages shown on menus that <paramref name="user"/> may view,
    /// the blog link when the site has a blog, then each provider's main items.
    /// </summary>
    /// <param name="user">The visitor. Pages they cannot view are omitted.</param>
    /// <param name="includeHomePageLink"><see langword="true"/> to add a home link at position -1.</param>
    /// <returns>The items, ordered by position and then text.</returns>
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

        if (siteSettings.EnableBlog)
        {
            menuItems.Add(BlogLink());
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

    /// <summary>
    /// Builds the submenu for the current slug: top-level pages when the slug is empty, otherwise the current page's children.
    /// Provider submenu items are included when the slug starts with the provider's root slug.
    /// </summary>
    /// <param name="user">The visitor. Pages they cannot view are omitted.</param>
    /// <param name="currentUrlSlug">The current slug, without a leading slash. <see langword="null"/> is treated as empty.</param>
    /// <returns>The items, ordered by position and then text. Empty when the slug is not a CMS page and no provider contributes.</returns>
    public async Task<IReadOnlyList<MenuItem>> BuildSubMenuAsync(ClaimsPrincipal user, string currentUrlSlug)
    {
        var menuItems = new List<MenuItem>();
        int tenantId = workContext.CurrentTenant.Id;
        currentUrlSlug ??= string.Empty;

        if (string.IsNullOrEmpty(currentUrlSlug))
        {
            if (siteSettings.EnableBlog)
            {
                menuItems.Add(BlogLink());
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

    private MenuItem BlogLink() => new()
    {
        Id = Guid.NewGuid(),
        Text = localizer[InfernoCmsLocalizableStrings.Blog.Title],
        Url = "/blog",
        Enabled = true,
        Position = 0
    };
}
