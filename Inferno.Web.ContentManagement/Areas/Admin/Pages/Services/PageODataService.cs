using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.OData;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;

public class PageODataService : RadzenODataService<Page, Guid>
{
    public PageODataService()
        : base($"{CmsConstants.ODataRoutes.Prefix}/{CmsConstants.ODataRoutes.EntitySetNames.Page}")
    {
    }
}

public class PageTypeODataService : RadzenODataService<PageType, Guid>
{
    public PageTypeODataService()
        : base($"{CmsConstants.ODataRoutes.Prefix}/{CmsConstants.ODataRoutes.EntitySetNames.PageType}")
    {
    }
}