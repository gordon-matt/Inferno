using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Tenants.Entities;

namespace Inferno.Tenants.Services;

/// <inheritdoc cref="ITenantService" />
public class TenantService : GenericDataService<Tenant>, ITenantService
{
    /// <summary>Passes the cache and the tenant repository to the generic data service.</summary>
    /// <param name="cacheManager">The cache used by the unfiltered and primary-key finds.</param>
    /// <param name="repository">The tenant store.</param>
    public TenantService(ICacheManager cacheManager, IRepository<Tenant> repository)
        : base(cacheManager, repository)
    {
    }
}