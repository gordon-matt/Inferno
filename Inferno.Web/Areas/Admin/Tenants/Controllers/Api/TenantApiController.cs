using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Inferno.Helpers;
using Inferno.Security;
using Inferno.Security.Membership;
using Inferno.Tenants.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Inferno.Web.Areas.Tenants.Controllers.Api
{
    [Authorize]
    public class TenantApiController : BaseODataController<Tenant, int>
    {
        private readonly IMembershipService membershipService;

        public TenantApiController(
            IAuthorizationService authorizationService,
            IRepository<Tenant> repository,
            IMembershipService membershipService)
            : base(authorizationService, repository)
        {
            this.membershipService = membershipService;
        }

        public override Task<IActionResult> Get(ODataQueryOptions<Tenant> options)
        {
            return base.Get(options);
        }

        public override async Task<IActionResult> Post([FromBody] Tenant entity)
        {
            var result = await base.Post(entity);
            int tenantId = entity.Id; // EF should have populated the ID in base.Post()
            await membershipService.EnsureAdminRoleForTenantAsync(tenantId);

            //TOOD: Create tenant media folder:
            var mediaFolder = new DirectoryInfo(CommonHelper.MapPath("~/Media/Uploads/Tenant_" + tenantId));
            if (!mediaFolder.Exists)
            {
                mediaFolder.Create();
            }

            return result;
        }

        public override async Task<IActionResult> Delete(int key)
        {
            var result = await base.Delete(key);

            //TODO: Remove everything associated with the tenant.

            // TODO: Add some checkbox on admin page... only delete files if user checks that box.
            //var mediaFolder = new DirectoryInfo(webHelper.MapPath("~/Media/Uploads/Tenant_" + key));
            //if (mediaFolder.Exists)
            //{
            //    mediaFolder.Delete();
            //}

            return result;
        }

        protected override int GetId(Tenant entity)
        {
            return entity.Id;
        }

        protected override void SetNewId(Tenant entity)
        {
        }

        protected override string ReadPermission => StandardPolicies.FullAccess;

        protected override string WritePermission => StandardPolicies.FullAccess;
    }
}