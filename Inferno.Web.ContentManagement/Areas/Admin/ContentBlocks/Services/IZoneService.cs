using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services
{
    public interface IZoneService : IGenericDataService<Zone>
    {
    }

    public class ZoneService : GenericDataService<Zone>, IZoneService
    {
        public ZoneService(ICacheManager cacheManager, IRepository<Zone> repository)
            : base(cacheManager, repository)
        {
        }
    }
}