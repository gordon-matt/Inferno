using Inferno.Localization.Entities;
using Inferno.Web.OData;

namespace Inferno.Web.Areas.Admin.Localization.Services
{
    public class LocalizableStringODataService : RadzenODataService<LocalizableString, Guid>
    {
        public LocalizableStringODataService()
            : base($"{InfernoWebConstants.ODataRoutes.Prefix}/{InfernoWebConstants.ODataRoutes.EntitySetNames.LocalizableString}")
        {
        }
    }
}