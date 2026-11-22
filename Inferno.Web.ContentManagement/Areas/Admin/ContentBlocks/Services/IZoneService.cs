using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;

/// <summary>Stores named content zones for a tenant.</summary>
public interface IZoneService : IGenericDataService<Zone>
{
}

/// <inheritdoc cref="IZoneService" />
public class ZoneService : GenericDataService<Zone>, IZoneService
{
    /// <summary>Passes the cache and the zone repository to the generic data service.</summary>
    /// <param name="cacheManager">The cache used by the unfiltered and primary-key finds.</param>
    /// <param name="repository">The zone store.</param>
    public ZoneService(ICacheManager cacheManager, IRepository<Zone> repository)
        : base(cacheManager, repository)
    {
    }
}