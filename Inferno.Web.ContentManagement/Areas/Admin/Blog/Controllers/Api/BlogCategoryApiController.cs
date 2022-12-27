using Extenso.Data.Entity;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Entities;
using Inferno.Web.OData;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Controllers.Api
{
    public class BlogCategoryApiController : GenericTenantODataController<BlogCategory, int>
    {
        public BlogCategoryApiController(IRepository<BlogCategory> repository)
            : base(repository)
        {
        }

        protected override int GetId(BlogCategory entity) => entity.Id;

        protected override void SetNewId(BlogCategory entity)
        {
        }

        protected override string ReadPermission => CmsConstants.Policies.BlogRead;

        protected override string WritePermission => CmsConstants.Policies.BlogWrite;
    }
}