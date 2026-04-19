using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Logging.Entities;

namespace Inferno.Logging.Services
{
    public class LogService : GenericDataService<LogEntry>, ILogService
    {
        public LogService(ICacheManager cacheManager, IRepository<LogEntry> repository)
            : base(cacheManager, repository)
        {
        }
    }
}
