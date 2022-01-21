using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Localization.Entities;

namespace Inferno.Localization.Services
{
    public class LocalizableStringService : GenericDataService<LocalizableString>, ILocalizableStringService
    {
        public LocalizableStringService(
            ICacheManager cacheManager,
            IRepository<LocalizableString> repository)
            : base(cacheManager, repository)
        {
        }
    }
}