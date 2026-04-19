using Inferno.Data.Services;
using Inferno.Logging.Entities;

namespace Inferno.Logging.Services
{
    public interface ILogService : IGenericDataService<LogEntry>
    {
    }
}
