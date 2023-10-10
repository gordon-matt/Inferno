using Dependo;
using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Localization;
using Inferno.Localization.Entities;
using Inferno.Localization.Services;
using Inferno.Web.OData;
using Inferno.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using LanguageEntity = Inferno.Localization.Entities.Language;

namespace Inferno.Web.Areas.Admin.Localization.Controllers.Api
{
    public class LanguageApiController : GenericTenantODataController<LanguageEntity, Guid>
    {
        private readonly Lazy<ICacheManager> cacheManager;
        private readonly Lazy<ILocalizableStringService> localizableStringService;

        public LanguageApiController(
            IAuthorizationService authorizationService,
            IRepository<LanguageEntity> repository,
            Lazy<ILocalizableStringService> localizableStringService,
            Lazy<ICacheManager> cacheManager)
            : base(authorizationService, repository)
        {
            this.localizableStringService = localizableStringService;
            this.cacheManager = cacheManager;
        }

        protected override Guid GetId(LanguageEntity entity) => entity.Id;

        protected override void SetNewId(LanguageEntity entity) => entity.Id = Guid.NewGuid();

        [HttpPost]
        public virtual async Task<IActionResult> ResetLocalizableStrings([FromBody] ODataActionParameters parameters)
        {
            if (!await AuthorizeAsync(WritePermission))
            {
                return Unauthorized();
            }

            int tenantId = GetTenantId();
            await localizableStringService.Value.DeleteAsync(x => x.TenantId == tenantId);

            var languagePacks = EngineContext.Current.ResolveAll<ILanguagePack>();

            var toInsert = new HashSet<LocalizableString>();
            foreach (var languagePack in languagePacks)
            {
                foreach (var localizedString in languagePack.LocalizedStrings)
                {
                    toInsert.Add(new LocalizableString
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        CultureCode = languagePack.CultureCode,
                        TextKey = localizedString.Key,
                        TextValue = localizedString.Value
                    });
                }
            }
            await localizableStringService.Value.InsertAsync(toInsert);

            cacheManager.Value.RemoveByPattern(CacheKeys.LocalizableStringsPatternFormat);

            return Ok();
        }

        protected override string ReadPermission => InfernoWebPolicies.LanguagesRead;

        protected override string WritePermission => InfernoWebPolicies.LanguagesWrite;
    }
}