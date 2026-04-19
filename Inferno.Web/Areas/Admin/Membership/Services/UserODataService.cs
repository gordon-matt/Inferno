using Inferno.Security.Membership;
using Inferno.Web.OData;

namespace Inferno.Web.Areas.Admin.Membership.Services
{
    public class UserODataService : RadzenODataService<InfernoUser, string>
    {
        public UserODataService()
            : base($"{InfernoWebConstants.ODataRoutes.Prefix}/{InfernoWebConstants.ODataRoutes.EntitySetNames.MembershipUser}")
        {
        }
    }
}