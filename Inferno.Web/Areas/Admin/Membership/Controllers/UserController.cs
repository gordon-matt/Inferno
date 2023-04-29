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
    [Route("admin/membership/users")]
    public class UserController : ExportController<InfernoUser>
    {
        private readonly IMembershipService membershipService;

        public UserController(IMembershipService membershipService)
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
            var users = await membershipService.GetAllUsersAsync(WorkContext.Value.CurrentTenant.Id);
            return Download(ApplyQuery(users.AsQueryable(), Request.Query), new DownloadOptions
            {
                FileFormat = fileFormat
            });
        }
    }
}