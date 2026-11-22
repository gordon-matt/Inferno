using Inferno.Web.ContentManagement.Areas.Admin.Pages;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Inferno.Web.Navigation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Inferno.Web.ContentManagement.Components;

/// <summary>Builds the breadcrumb trail for a public page or the blog.</summary>
public class CmsBreadcrumbBuilder
{
    private readonly IPageService pageService;
    private readonly IPageVersionService pageVersionService;
    private readonly IWorkContext workContext;
    private readonly IStringLocalizer localizer;

    /// <summary>Stores the page services, work context, and localizer used to build a trail.</summary>
    /// <param name="pageService">Loads enabled pages so parent links can be resolved.</param>
    /// <param name="pageVersionService">Loads the current version and its ancestors.</param>
    /// <param name="workContext">Supplies the tenant, culture, and any breadcrumbs already set for the request.</param>
    /// <param name="localizer">Supplies the blog caption.</param>
    public CmsBreadcrumbBuilder(
        IPageService pageService,
        IPageVersionService pageVersionService,
        IWorkContext workContext,
        IStringLocalizer localizer)
    {
        this.pageService = pageService;
        this.pageVersionService = pageVersionService;
        this.workContext = workContext;
        this.localizer = localizer;
    }

    /// <summary>Builds the trail for <paramref name="path"/>.</summary>
    /// <param name="path">The request path. Leading and trailing slashes are ignored. <see langword="null"/> is treated as empty.</param>
    /// <param name="user">The visitor. Ancestor pages they cannot view are omitted. The current page is not checked.</param>
    /// <returns>
    /// For a blog path, the blog crumb and, below the blog index, the last crumb already on the work context.
    /// For a known page, its ancestors and then the current title. Otherwise the crumbs already on the work context.
    /// </returns>
    public async Task<IReadOnlyList<Breadcrumb>> BuildAsync(string path, System.Security.Principal.IPrincipal user)
    {
        string slug = (path ?? string.Empty).Trim('/');
        var breadcrumbs = new List<Breadcrumb>();

        if (slug.Equals("blog", StringComparison.OrdinalIgnoreCase)
            || slug.StartsWith("blog/", StringComparison.OrdinalIgnoreCase))
        {
            breadcrumbs.Add(new Breadcrumb
            {
                Text = localizer[InfernoCmsLocalizableStrings.Blog.Title],
                Url = slug.Equals("blog", StringComparison.OrdinalIgnoreCase) ? null : "/blog"
            });

            if (!slug.Equals("blog", StringComparison.OrdinalIgnoreCase) && workContext.Breadcrumbs.Count > 0)
            {
                breadcrumbs.Add(workContext.Breadcrumbs.Last());
            }

            return breadcrumbs;
        }

        int tenantId = workContext.CurrentTenant.Id;
        PageVersion current;
        using (var connection = pageVersionService.OpenConnection())
        {
            current = await connection.Query()
                .Include(x => x.Page)
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Slug == slug);
        }

        if (current == null)
        {
            return workContext.Breadcrumbs.ToList();
        }

        var pages = pageService.Find(new Extenso.Data.Entity.SearchOptions<Page>
        {
            Query = x => x.TenantId == tenantId && x.IsEnabled
        }).ToList();

        var trail = new List<Breadcrumb>();
        Guid? parentId = current.Page.ParentId;
        while (parentId != null)
        {
            var parent = pages.FirstOrDefault(x => x.Id == parentId);
            if (parent == null)
            {
                break;
            }

            if (await PageSecurityHelper.CheckUserHasAccessToPage(parent, user))
            {
                var parentVersion = pageVersionService.GetCurrentVersion(tenantId, parent.Id, workContext.CurrentCultureCode, enabledOnly: true, shownOnMenusOnly: false);
                if (parentVersion != null)
                {
                    trail.Add(new Breadcrumb
                    {
                        Text = parentVersion.Title,
                        Url = "/" + parentVersion.Slug
                    });
                }
            }

            parentId = parent.ParentId;
        }

        trail.Reverse();
        trail.Add(new Breadcrumb { Text = current.Title });
        return trail;
    }
}
