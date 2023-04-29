using Inferno.Security.Membership;
using Inferno.Web.OData;

namespace Inferno.Web.Areas.Admin.Membership.Services
{
    public class RoleODataService : RadzenODataService<InfernoRole, string>
    {
        public RoleODataService()
            : base($"{InfernoWebConstants.ODataRoutes.Prefix}/{InfernoWebConstants.ODataRoutes.EntitySetNames.MembershipRole}")
        {
        }
    }
}
