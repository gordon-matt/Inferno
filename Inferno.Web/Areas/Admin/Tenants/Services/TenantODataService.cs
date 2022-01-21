using Inferno.Tenants.Entities;
using Inferno.Web.OData;
using Microsoft.AspNetCore.Http;

namespace Inferno.Web.Areas.Admin.Tenants.Services
{
    public class TenantODataService : RadzenODataService<Tenant>
    {
        public TenantODataService(IHttpContextAccessor httpContextAccessor)
            : base("inferno/web/TenantApi", httpContextAccessor)
        {
        }
    }
}