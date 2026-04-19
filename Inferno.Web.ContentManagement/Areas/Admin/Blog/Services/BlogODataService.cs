using Inferno.Web.ContentManagement.Areas.Admin.Blog.Entities;
using Inferno.Web.OData;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Services;

public class BlogCategoryODataService : RadzenODataService<BlogCategory, int>
{
    public BlogCategoryODataService()
        : base($"{CmsConstants.ODataRoutes.Prefix}/{CmsConstants.ODataRoutes.EntitySetNames.BlogCategory}")
    {
    }
}

public class BlogTagODataService : RadzenODataService<BlogTag, int>
{
    public BlogTagODataService()
        : base($"{CmsConstants.ODataRoutes.Prefix}/{CmsConstants.ODataRoutes.EntitySetNames.BlogTag}")
    {
    }
}

public class BlogPostODataService : RadzenODataService<BlogPost, Guid>
{
    public BlogPostODataService()
        : base($"{CmsConstants.ODataRoutes.Prefix}/{CmsConstants.ODataRoutes.EntitySetNames.BlogPost}")
    {
    }
}