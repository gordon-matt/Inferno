using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Models;
using Inferno.Web.OData;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Services;

public class SubscriberODataService : RadzenODataService<Subscriber, string>
{
    public SubscriberODataService()
        : base($"{CmsConstants.ODataRoutes.Prefix}/{CmsConstants.ODataRoutes.EntitySetNames.Subscriber}")
    {
    }
}