using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.OData;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;

public class EntityTypeContentBlockODataService : RadzenODataService<EntityTypeContentBlock, Guid>
{
    public EntityTypeContentBlockODataService()
        : base($"{CmsConstants.ODataRoutes.Prefix}/{CmsConstants.ODataRoutes.EntitySetNames.EntityTypeContentBlock}")
    {
    }
}
