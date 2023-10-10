using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Web.Configuration;
using Inferno.Web.Configuration.Entities;
using Inferno.Web.OData;
using Inferno.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;

namespace Inferno.Web.Areas.Admin.Configuration.Controllers.Api
{
    public class SettingsApiController : GenericTenantODataController<Setting, Guid>
    {
        private readonly ICacheManager cacheManager;

        public SettingsApiController(IAuthorizationService authorizationService, IRepository<Setting> repository, ICacheManager cacheManager)
            : base(authorizationService, repository)
        {
            this.cacheManager = cacheManager;
        }

        protected override Guid GetId(Setting entity) => entity.Id;

        protected override void SetNewId(Setting entity) => entity.Id = Guid.NewGuid();

        public override async Task<IActionResult> Put([FromODataUri] Guid key, [FromBody] Setting entity)
        {
            var result = await base.Put(key, entity);

            string cacheKey = string.Format(InfernoWebConstants.CacheKeys.SettingsKeyFormat, entity.TenantId, entity.Type);
            cacheManager.Remove(cacheKey);

            // TODO: This is an ugly hack. We need to have a way for each setting to perform some tasks after update
            if (entity.Name == new SiteSettings().Name)
            {
                cacheManager.Remove(InfernoWebConstants.CacheKeys.CurrentCulture);
            }

            return result;
        }

        public override async Task<IActionResult> Post([FromBody] Setting entity)
        {
            var result = await base.Post(entity);

            string cacheKey = string.Format(InfernoWebConstants.CacheKeys.SettingsKeyFormat, entity.TenantId, entity.Type);
            cacheManager.Remove(cacheKey);

            return result;
        }

        public override async Task<IActionResult> Patch([FromODataUri] Guid key, Delta<Setting> patch)
        {
            var result = await base.Patch(key, patch);

            var entity = await Repository.FindOneAsync(key);
            string cacheKey = string.Format(InfernoWebConstants.CacheKeys.SettingsKeyFormat, entity.TenantId, entity.Type);
            cacheManager.Remove(cacheKey);

            return result;
        }

        public override async Task<IActionResult> Delete([FromODataUri] Guid key)
        {
            var result = base.Delete(key);

            var entity = await Repository.FindOneAsync(key);
            string cacheKey = string.Format(InfernoWebConstants.CacheKeys.SettingsKeyFormat, entity.TenantId, entity.Type);
            cacheManager.Remove(cacheKey);

            return await result;
        }

        protected override string ReadPermission => InfernoWebPolicies.SettingsRead;

        protected override string WritePermission => InfernoWebPolicies.SettingsWrite;
    }
}