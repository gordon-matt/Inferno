using Extenso.Data.Entity;
using Inferno.Localization.Entities;
using Inferno.Web.Areas.Admin.Localization.Models;
using Inferno.Web.Configuration;
using Inferno.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace Inferno.Web.Areas.Admin.Localization.Controllers
{
    [Area(InfernoWebConstants.Areas.Localization)]
    [Route("admin/localization/localizable-strings")]
    public class LocalizableStringController : ExportController<LocalizableString>
    {
        private readonly IRepository<LocalizableString> repository;
        private readonly SiteSettings siteSettings;

        public LocalizableStringController(IRepository<LocalizableString> repository, SiteSettings siteSettings)
        {
            this.repository = repository;
            this.siteSettings = siteSettings;
        }

        [HttpGet("export/{cultureCode}")]
        public async Task<FileResult> ExportToJson(string cultureCode)
        {
            int tenantId = WorkContext.Value.CurrentTenant.Id;

            var localizedStrings = await repository.FindAsync(x =>
                x.TenantId == tenantId &&
                x.CultureCode == cultureCode &&
                x.TextValue != null);

            var languagePack = new LanguagePackFile
            {
                CultureCode = cultureCode,
                LocalizedStrings = localizedStrings.ToDictionary(k => k.TextKey, v => v.TextValue)
            };

            using var connection = repository.OpenConnection();
            string fileName = string.Format("{0}_LanguagePack_{1}_{2:yyyy-MM-dd}.json", siteSettings.SiteName, cultureCode, DateTime.Now);
            return DownloadJson(languagePack, fileName);
        }
    }
}