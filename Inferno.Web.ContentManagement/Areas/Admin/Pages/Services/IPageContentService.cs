using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;

/// <summary>Finds the version of a page that a public request should render.</summary>
public interface IPageContentService
{
    /// <summary>Finds the published version for a slug.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="slug">The page slug. Whitespace returns <see langword="null"/>.</param>
    /// <param name="cultureCode">The culture passed to <see cref="Inferno.Web.ContentManagement.Areas.Admin.Pages.PageVersionSelector.Select"/>.</param>
    /// <returns>The selected version when its page is enabled; otherwise <see langword="null"/>.</returns>
    Task<PageVersion> FindPublishedAsync(int tenantId, string slug, string cultureCode);
}

/// <inheritdoc cref="IPageContentService" />
public class PageContentService : IPageContentService
{
    private readonly IPageVersionService pageVersionService;

    /// <summary>Stores the page-version service used to load versions by slug.</summary>
    /// <param name="pageVersionService">Opens a connection over the page versions.</param>
    public PageContentService(IPageVersionService pageVersionService)
    {
        this.pageVersionService = pageVersionService;
    }

    /// <inheritdoc />
    public async Task<PageVersion> FindPublishedAsync(int tenantId, string slug, string cultureCode)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        List<PageVersion> versions;
        using (var connection = pageVersionService.OpenConnection())
        {
            versions = await connection.Query()
                .Include(x => x.Page)
                .Where(x => x.TenantId == tenantId && x.Slug == slug)
                .ToListAsync();
        }

        var selected = PageVersionSelector.Select(versions, cultureCode);
        if (selected?.Page == null || !selected.Page.IsEnabled)
        {
            return null;
        }

        return selected;
    }
}
