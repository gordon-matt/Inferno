using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Tenants.Entities;

namespace Inferno.Tenants.Services
{
    public class TenantService : GenericDataService<Tenant>, ITenantService
    {
        public TenantService(ICacheManager cacheManager, IRepository<Tenant> repository)
            : base(cacheManager, repository)
        {
        }
    }
}