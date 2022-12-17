using Inferno.Tenants.Entities;
using Inferno.Web.OData;

namespace Inferno.Web.Areas.Tenants.Services
{
    public class TenantODataService : RadzenODataService<Tenant>
    {
        public TenantODataService() : base("inferno/web/TenantApi")
        {
        }
    }
}