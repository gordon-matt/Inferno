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
/// <param name="subscriberAdminService">Loads the subscribers to export.</param>
/// <param name="workContext">Supplies the current tenant.</param>
public class SubscriberController(ISubscriberAdminService subscriberAdminService, IWorkContext workContext) : ExportController<Subscriber>
{
    /// <summary>Downloads the current tenant's subscribers as a delimited file.</summary>
    /// <param name="search">Matches the name or email. Whitespace exports everyone who is subscribed.</param>
    /// <returns>The file, or a problem result when the subscribers cannot be loaded.</returns>
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
