using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.OData;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services
{
    public class ContentBlockODataService : RadzenODataService<ContentBlock, Guid>
    {
        public ContentBlockODataService()
            : base($"{CmsConstants.ODataRoutes.Prefix}/{CmsConstants.ODataRoutes.EntitySetNames.ContentBlock}")
        {
        }
    }
}