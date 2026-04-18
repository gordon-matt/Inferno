using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Inferno.Web.OData;

namespace Inferno.Web.ContentManagement.Areas.Admin.Menus.Services
{
    public class MenuODataService : RadzenODataService<Menu, Guid>
    {
        public MenuODataService()
            : base($"{CmsConstants.ODataRoutes.Prefix}/{CmsConstants.ODataRoutes.EntitySetNames.Menu}")
        {
        }
    }

    public class MenuItemODataService : RadzenODataService<MenuItem, Guid>
    {
        public MenuItemODataService()
            : base($"{CmsConstants.ODataRoutes.Prefix}/{CmsConstants.ODataRoutes.EntitySetNames.MenuItem}")
        {
        }
    }
}
