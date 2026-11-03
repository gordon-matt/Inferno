using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Models;
using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Services;
using Inferno.Web.Models;
using Inferno.Web.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Controllers;

/// <summary>File downloads for the subscribers screen, which a Blazor circuit cannot stream to the browser.</summary>
[Authorize(Policy = CmsConstants.Policies.NewsletterRead)]
[Route("admin/newsletters/subscribers")]
public class SubscriberController(ISubscriberAdminService subscriberAdminService, IWorkContext workContext) : ExportController<Subscriber>
{
    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportToCsv(string search)
    {
        var result = await subscriberAdminService.ListAsync(workContext.CurrentTenant.Id, search);
        if (!result.IsSuccess)
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError);
        }

        return Download(result.Value.AsQueryable(), new DownloadOptions { FileFormat = DownloadFileFormat.Delimited });
    }
}
