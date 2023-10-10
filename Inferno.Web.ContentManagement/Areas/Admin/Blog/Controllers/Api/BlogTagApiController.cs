using Extenso.Data.Entity;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Entities;
using Inferno.Web.OData;
using Microsoft.AspNetCore.Authorization;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Controllers.Api
{
    public class BlogTagApiController : GenericTenantODataController<BlogTag, int>
    {
        public BlogTagApiController(IAuthorizationService authorizationService, IRepository<BlogTag> repository)
            : base(authorizationService, repository)
        {
        }

        protected override int GetId(BlogTag entity)
        {
            return entity.Id;
        }

        protected override void SetNewId(BlogTag entity)
        {
        }

        protected override string ReadPermission => CmsConstants.Policies.BlogRead;

        protected override string WritePermission => CmsConstants.Policies.BlogWrite;
    }
}