using Inferno.Security;
using Inferno.Tenants.Entities;
using Inferno.Web.Areas.Admin.Tenants.Services;
using Inferno.Web.Models;
using Inferno.Web.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inferno.Web.Areas.Tenants.Controllers;

/// <summary>File downloads for the tenants screen, which a Blazor circuit cannot stream to the browser.</summary>
[Area(InfernoWebConstants.Areas.Tenants)]
[Authorize(Policy = StandardPolicies.FullAccess)]
[Route("admin/tenants")]
public class TenantController(ITenantAdminService tenantAdminService) : ExportController<Tenant>
{
    [HttpGet("export/csv")]
    public Task<IActionResult> ExportToCsv(string search) => ExportAsync(search, DownloadFileFormat.Delimited);

    [HttpGet("export/excel")]
    public Task<IActionResult> ExportToExcel(string search) => ExportAsync(search, DownloadFileFormat.XLSX);

    private async Task<IActionResult> ExportAsync(string search, DownloadFileFormat fileFormat)
    {
        var result = await tenantAdminService.ListAsync(search);
        if (!result.IsSuccess)
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError);
        }

        return Download(result.Value.AsQueryable(), new DownloadOptions { FileFormat = fileFormat });
    }
}
