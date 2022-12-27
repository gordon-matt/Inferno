using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Services
{
    public interface IBlogPostTagService : IGenericDataService<BlogPostTag>
    {
    }

    public class BlogPostTagService : GenericDataService<BlogPostTag>, IBlogPostTagService
    {
        public BlogPostTagService(ICacheManager cacheManager, IRepository<BlogPostTag> repository)
            : base(cacheManager, repository)
        {
        }
    }
}