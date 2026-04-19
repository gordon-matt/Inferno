using Extenso.Data.Entity;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.OData;
using Microsoft.AspNetCore.Authorization;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Controllers.Api
{
    //[Authorize(Roles = InfernoConstants.Roles.Administrators)]
    public class ZoneApiController : GenericTenantODataController<Zone, Guid>
    {
        public ZoneApiController(IAuthorizationService authorizationService, IRepository<Zone> repository)
            : base(authorizationService, repository)
        {
        }

        protected override Guid GetId(Zone entity) => entity.Id;

        protected override void SetNewId(Zone entity) => entity.Id = Guid.NewGuid();

        protected override string ReadPermission => CmsConstants.Policies.ContentZonesRead;

        protected override string WritePermission => CmsConstants.Policies.ContentZonesWrite;
    }
}