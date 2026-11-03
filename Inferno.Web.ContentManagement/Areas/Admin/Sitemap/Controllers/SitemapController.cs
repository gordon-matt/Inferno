using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Controllers;

public class SitemapController(IWorkContext workContext, ISitemapAdminService sitemapAdminService) : Controller
{
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
