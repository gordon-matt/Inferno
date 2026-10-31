using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;

public interface IPageContentService
{
    Task<PageVersion> FindPublishedAsync(int tenantId, string slug, string cultureCode);
}

public class PageContentService : IPageContentService
{
    private readonly IPageVersionService pageVersionService;

    public PageContentService(IPageVersionService pageVersionService)
    {
        this.pageVersionService = pageVersionService;
    }

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
