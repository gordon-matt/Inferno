using Inferno.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Controllers;

public class SitemapController : Controller
{
    private readonly IWorkContext workContext;

    public SitemapController(IWorkContext workContext)
    {
        this.workContext = workContext;
    }

    [Route("sitemap.xml")]
    public IActionResult SitemapXml()
    {
        if (workContext.CurrentTenant == null)
        {
            return NotFound();
        }

        string filePath = CommonHelper.MapPath($"~/sitemap-{workContext.CurrentTenant.Id}.xml");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        string fileContent = System.IO.File.ReadAllText(filePath);
        return Content(fileContent, "text/xml");
    }
}
