using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Inferno.Logging.Entities;
using Inferno.Security;
using Inferno.Web.OData;
using Inferno.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;

namespace Inferno.Web.Areas.Admin.Log.Controllers.Api
{
    [Authorize]
    public class LogApiController : GenericTenantODataController<LogEntry, int>
    {
        public LogApiController(
            IAuthorizationService authorizationService,
            IRepository<LogEntry> repository)
            : base(authorizationService, repository)
        {
        }

        protected override int GetId(LogEntry entity) => entity.Id;

        protected override void SetNewId(LogEntry entity)
        {
        }

        [HttpPost]
        public virtual async Task<IActionResult> Clear([FromBody] ODataActionParameters parameters)
        {
            if (!await AuthorizeAsync(WritePermission))
            {
                return Unauthorized();
            }

            int tenantId = GetTenantId();
            await Repository.DeleteAsync(x => x.TenantId == tenantId);

            return Ok();
        }

        protected override string ReadPermission => InfernoWebPolicies.LogRead;

        protected override string WritePermission => StandardPolicies.FullAccess;
    }
}
