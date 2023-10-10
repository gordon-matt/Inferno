using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Localization.Entities;
using Inferno.Web.Areas.Admin.Localization.Models;
using Inferno.Web.OData;
using Inferno.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;

namespace Inferno.Web.Areas.Admin.Localization.Controllers.Api
{
    public class LocalizableStringApiController : GenericTenantODataController<LocalizableString, Guid>
    {
        private readonly ICacheManager cacheManager;

        public LocalizableStringApiController(IAuthorizationService authorizationService, IRepository<LocalizableString> repository, ICacheManager cacheManager)
            : base(authorizationService, repository)
        {
            this.cacheManager = cacheManager;
        }

        protected override Guid GetId(LocalizableString entity) => entity.Id;

        protected override void SetNewId(LocalizableString entity) => entity.Id = Guid.NewGuid();

        //[EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Wrong Usage", "DF0010:Marks undisposed local variables.", Justification = "Disposable of the repository connection is handled by the `GenericODataController`.")]
        [HttpGet]
        public virtual async Task<IActionResult> GetComparitiveTable(
            [FromODataUri] string cultureCode,
            ODataQueryOptions<ComparitiveLocalizableString> options)
        {
            if (!await AuthorizeAsync(ReadPermission))
            {
                return Unauthorized();
            }
            else
            {
                int tenantId = GetTenantId();
                var connection = GetDisposableConnection();

                // With grouping, we use .Where() and then .FirstOrDefault() instead of just the .FirstOrDefault() by itself
                //  for compatibility with MySQL.
                //  See: http://stackoverflow.com/questions/23480044/entity-framework-select-statement-with-logic
                var query = connection.Query(x => x.TenantId == tenantId && (x.CultureCode == null || x.CultureCode == cultureCode))
                            .GroupBy(x => x.TextKey)
                            .Select(grp => new ComparitiveLocalizableString
                            {
                                Key = grp.Key,
                                InvariantValue = grp.Where(x => x.CultureCode == null).FirstOrDefault().TextValue,
                                LocalizedValue = grp.Where(x => x.CultureCode == cultureCode).FirstOrDefault() == null
                                    ? string.Empty
                                    : grp.Where(x => x.CultureCode == cultureCode).FirstOrDefault().TextValue
                            });

                var results = options.ApplyTo(query, IgnoreQueryOptions);
                var response = await Task.FromResult((results as IQueryable<ComparitiveLocalizableString>).ToHashSet());
                return Ok(response);
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> PutComparitive([FromBody] ODataActionParameters parameters)
        {
            if (!await AuthorizeAsync(WritePermission))
            {
                return Unauthorized();
            }

            string cultureCode = (string)parameters["cultureCode"];
            string key = (string)parameters["key"];
            var entity = (ComparitiveLocalizableString)parameters["entity"];

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!key.Equals(entity.Key))
            {
                return BadRequest();
            }

            int tenantId = GetTenantId();
            var localizedString = await Repository.FindOneAsync(x => x.TenantId == tenantId && x.CultureCode == cultureCode && x.TextKey == key);

            if (localizedString == null)
            {
                localizedString = new LocalizableString
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CultureCode = cultureCode,
                    TextKey = key,
                    TextValue = entity.LocalizedValue
                };
                await Repository.InsertAsync(localizedString);
            }
            else
            {
                localizedString.TextValue = entity.LocalizedValue;
                await Repository.UpdateAsync(localizedString);
            }

            cacheManager.Remove(string.Concat(Inferno.Localization.CacheKeys.LocalizableStringsFormat, tenantId, cultureCode));

            return Updated(entity);
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteComparitive([FromBody] ODataActionParameters parameters)
        {
            if (!await AuthorizeAsync(WritePermission))
            {
                return Unauthorized();
            }

            string cultureCode = (string)parameters["cultureCode"];
            string key = (string)parameters["key"];

            int tenantId = GetTenantId();
            var entity = await Repository.FindOneAsync(x => x.TenantId == tenantId && x.CultureCode == cultureCode && x.TextKey == key);
            if (entity == null)
            {
                return NotFound();
            }

            entity.TextValue = null;
            await Repository.UpdateAsync(entity);
            //Repository.Delete(entity);

            cacheManager.Remove(string.Concat(Inferno.Localization.CacheKeys.LocalizableStringsFormat, tenantId, cultureCode));

            return NoContent();
        }

        protected override string ReadPermission => InfernoWebPolicies.LanguagesRead;

        protected override string WritePermission => InfernoWebPolicies.LanguagesWrite;
    }
}