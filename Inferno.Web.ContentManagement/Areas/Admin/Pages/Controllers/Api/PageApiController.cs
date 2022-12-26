using Extenso.Data.Entity;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Inferno.Web.OData;
using Microsoft.AspNetCore.Mvc;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Controllers.Api
{
    public class PageApiController : GenericTenantODataController<Page, Guid>
    {
        private readonly IPageService service;

        public PageApiController(IRepository<Page> repository, IPageService service)
            : base(repository)
        {
            this.service = service;
        }

        protected override Guid GetId(Page entity) => entity.Id;

        protected override void SetNewId(Page entity) => entity.Id = Guid.NewGuid();

        protected override string ReadPermission => CmsConstants.Policies.PagesRead;

        protected override string WritePermission => CmsConstants.Policies.PagesWrite;

        [HttpGet]
        public async Task<IActionResult> GetTopLevelPages()
        {
            if (!await AuthorizeAsync(ReadPermission))
            {
                return Unauthorized();
            }

            int tenantId = GetTenantId();
            var topLevelPages = service.GetTopLevelPages(tenantId);

            return Ok(topLevelPages);
        }
    }
}