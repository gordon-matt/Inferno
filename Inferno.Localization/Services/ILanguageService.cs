﻿using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using LanguageEntity = Inferno.Localization.Entities.Language;

namespace Inferno.Localization.Services
{
    public class LanguageService : GenericDataService<LanguageEntity>, ILanguageService
    {
        public LanguageService(ICacheManager cacheManager, IRepository<LanguageEntity> repository)
            : base(cacheManager, repository)
        {
        }

        public IEnumerable<LanguageEntity> GetActiveLanguages(int tenantId)
        {
            return Find(x => x.TenantId == tenantId && x.IsEnabled);
        }

        public bool CheckIfRightToLeft(int tenantId, string cultureCode)
        {
            var rtlLanguages = CacheManager.Get("Repository_Language_RightToLeft_" + tenantId, () =>
            {
                using var connection = OpenConnection();
                return connection.Query(x => x.TenantId == tenantId && x.IsRTL)
                    .Select(k => k.CultureCode)
                    .ToList();
            });

            return rtlLanguages.Contains(cultureCode);
        }

        protected override void ClearCache()
        {
            base.ClearCache();
            CacheManager.RemoveByPattern("Repository_Language_RightToLeft_.*");
        }
    }
}