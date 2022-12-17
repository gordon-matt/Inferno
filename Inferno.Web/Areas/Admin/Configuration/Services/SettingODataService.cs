using Inferno.Web.Configuration.Entities;
using Inferno.Web.OData;

namespace Inferno.Web.Areas.Admin.Configuration.Services
{
    public class SettingODataService : RadzenODataService<Setting, Guid>
    {
        public SettingODataService()
            : base($"{InfernoWebConstants.ODataRoutes.Prefix}/{InfernoWebConstants.ODataRoutes.EntitySetNames.Settings}")
        {
        }
    }
}