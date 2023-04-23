using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PageTypeApiController : BaseODataController<PageType, Guid>
    {
        public PageTypeApiController(IRepository<PageType> repository)
            : base(repository)
        {
        }

        protected override Guid GetId(PageType entity) => entity.Id;

        protected override void SetNewId(PageType entity) => entity.Id = Guid.NewGuid();

        protected override string ReadPermission => CmsConstants.Policies.PageTypesRead;

        protected override string WritePermission => CmsConstants.Policies.PageTypesWrite;
    }
}