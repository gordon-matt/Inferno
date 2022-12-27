using Extenso.Data.Entity;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Inferno.Web.OData;

namespace Inferno.Web.ContentManagement.Areas.Admin.Menus.Controllers.Api
{
    //[Authorize(Roles = InfernoConstants.Roles.Administrators)]
    public class MenuApiController : GenericTenantODataController<Menu, Guid>
    {
        public MenuApiController(IRepository<Menu> repository)
            : base(repository)
        {
        }

        protected override Guid GetId(Menu entity) => entity.Id;

        protected override void SetNewId(Menu entity) => entity.Id = Guid.NewGuid();

        protected override string ReadPermission => CmsConstants.Policies.MenusRead;

        protected override string WritePermission => CmsConstants.Policies.MenusWrite;
    }
}