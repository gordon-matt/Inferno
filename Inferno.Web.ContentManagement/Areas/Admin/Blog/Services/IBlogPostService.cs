using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Services
{
    public interface IBlogPostService : IGenericDataService<BlogPost>
    {
    }

    public class BlogPostService : GenericDataService<BlogPost>, IBlogPostService
    {
        public BlogPostService(ICacheManager cacheManager, IRepository<BlogPost> repository)
            : base(cacheManager, repository)
        {
        }
    }
}