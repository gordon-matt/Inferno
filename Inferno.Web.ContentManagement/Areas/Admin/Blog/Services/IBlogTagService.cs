using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Services
{
    public interface IBlogTagService : IGenericDataService<BlogTag>
    {
    }

    public class BlogTagService : GenericDataService<BlogTag>, IBlogTagService
    {
        public BlogTagService(ICacheManager cacheManager, IRepository<BlogTag> repository)
            : base(cacheManager, repository)
        {
        }
    }
}