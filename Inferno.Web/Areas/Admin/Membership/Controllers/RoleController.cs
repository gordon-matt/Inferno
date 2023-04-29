using Inferno.Security;
using Inferno.Security.Membership;
using Inferno.Web.Models;
using Inferno.Web.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inferno.Web.Areas.Admin.Membership.Controllers
{
    [Area(InfernoWebConstants.Areas.Membership)]
    [Authorize(Policy = StandardPolicies.FullAccess)]
    [Route("admin/membership/roles")]
    public class RoleController : ExportController<InfernoRole>
    {
        private readonly IMembershipService membershipService;

        public RoleController(IMembershipService membershipService)
        {
            this.membershipService = membershipService;
        }

        [HttpGet("export/csv")]
        public async Task<FileResult> ExportToCsv()
        {
            return await Export(DownloadFileFormat.Delimited);
        }

        [HttpGet("export/excel")]
        public async Task<FileResult> ExportToExcel()
        {
            return await Export(DownloadFileFormat.XLSX);
        }

        private async Task<FileResult> Export(DownloadFileFormat fileFormat)
        {
            var roles = await membershipService.GetAllRolesAsync(WorkContext.Value.CurrentTenant.Id);
            return Download(ApplyQuery(roles.AsQueryable(), Request.Query), new DownloadOptions
            {
                FileFormat = fileFormat
            });
        }
    }
}