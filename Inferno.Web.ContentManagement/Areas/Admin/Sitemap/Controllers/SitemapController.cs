using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Controllers;

/// <summary>Serves the current tenant's generated sitemap file.</summary>
/// <param name="workContext">Supplies the current tenant. A missing tenant is not found.</param>
/// <param name="sitemapAdminService">Locates the generated file.</param>
public class SitemapController(IWorkContext workContext, ISitemapAdminService sitemapAdminService) : Controller
{
    /// <summary>Returns the tenant's sitemap XML.</summary>
    /// <returns>The file as XML, or not found when there is no tenant or the file has not been generated.</returns>
    [Route("sitemap.xml")]
    public async Task<IActionResult> SitemapXml()
    {
        if (workContext.CurrentTenant == null)
        {
            return NotFound();
        }

        string filePath = sitemapAdminService.GetFilePath(workContext.CurrentTenant.Id);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        string fileContent = await System.IO.File.ReadAllTextAsync(filePath);
        return Content(fileContent, "text/xml");
    }
}
