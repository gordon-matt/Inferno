﻿using System.Globalization;
using Dependo;
using Extenso.Collections;
using Inferno.Caching;
using Inferno.Localization;
using Inferno.Localization.Entities;
using Inferno.Localization.Services;
using Microsoft.Extensions.Localization;

namespace Inferno.Web.Localization
{
    public class InfernoStringLocalizer : IStringLocalizer
    {
        private CultureInfo culture;
        private readonly ICacheManager cacheManager;
        private readonly ILocalizableStringService localizableStringService;
        private readonly IWorkContext workContext;
        private readonly object objSync = new();

        public CultureInfo Culture
        {
            get => culture ??= CultureInfo.CurrentCulture;
            private set => culture = value;
        }

        public InfernoStringLocalizer(
            ICacheManager cacheManager,
            IWorkContext workContext,
            ILocalizableStringService localizableStringService)
        {
            this.cacheManager = cacheManager;
            this.workContext = workContext;
            this.localizableStringService = localizableStringService;
        }

        public LocalizedString this[string name]=> GetLocalizedString(name);

        public LocalizedString this[string name, params object[] arguments]=> GetLocalizedString(name);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            int tenantId = workContext.CurrentTenant.Id;
            string cultureCode = Culture.Name;

            var resourceCache = LoadCulture(tenantId, cultureCode);

            return resourceCache.IsNullOrEmpty()
                ? Enumerable.Empty<LocalizedString>()
                : resourceCache.Select(x => new LocalizedString(x.Key, x.Value));
        }

        public IStringLocalizer WithCulture(CultureInfo culture) =>
            new InfernoStringLocalizer(cacheManager, workContext, localizableStringService)
            {
                Culture = culture
            };

        public virtual LocalizedString GetLocalizedString(string key)
        {
            int tenantId = workContext.CurrentTenant.Id;
            string cultureCode = Culture.Name;

            lock (objSync)
            {
                var resourceCache = LoadCulture(tenantId, cultureCode);
                if (resourceCache.TryGetValue(key, out string resourceCacheValue))
                {
                    return new LocalizedString(key, resourceCacheValue);
                }

                var invariantResourceCache = LoadCulture(tenantId, null);
                if (invariantResourceCache.TryGetValue(key, out string invariantResourceCacheValue))
                {
                    return new LocalizedString(key, invariantResourceCacheValue);
                }

                string value = AddTranslation(tenantId, null, key);
                invariantResourceCache.Add(key, value);
            }

            return new LocalizedString(key, key);
        }

        protected virtual IDictionary<string, string> LoadCulture(int tenantId, string cultureCode)
        {
            string cacheKey = string.Concat(CacheKeys.LocalizableStringsFormat, tenantId, cultureCode);
            return cacheManager.Get(cacheKey, () =>
            {
                return LoadTranslationsForCulture(tenantId, cultureCode);
            });
        }

        protected virtual Dictionary<string, string> LoadTranslationsForCulture(int tenantId, string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                return LoadTranslations(localizableStringService.Find(x => x.TenantId == tenantId && x.CultureCode == null));
            }

            return LoadTranslations(localizableStringService.Find(x => x.TenantId == tenantId && x.CultureCode == cultureCode));
        }

        private static Dictionary<string, string> LoadTranslations(IEnumerable<LocalizableString> items)
        {
            var dictionary = new Dictionary<string, string>();

            foreach (var item in items.Where(item => !dictionary.ContainsKey(item.TextKey)))
            {
                dictionary.Add(item.TextKey, item.TextValue);
            }

            return dictionary;
        }

        protected virtual string AddTranslation(int tenantId, string cultureCode, string key)
        {
            // TODO: Consider resolving this once for better performance?
            var providers = EngineContext.Current.ResolveAll<ILanguagePack>();
            var languagePacks = providers.Where(x => x.CultureCode == null);

            string value = key;

            foreach (var languagePack in languagePacks)
            {
                if (languagePack.LocalizedStrings.ContainsKey(key))
                {
                    value = languagePack.LocalizedStrings[key];
                    break;
                }
            }

            localizableStringService.Insert(new LocalizableString
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CultureCode = cultureCode,
                TextKey = key,
                TextValue = value
            });
            return value;
        }
    }
}