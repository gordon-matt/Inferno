using Inferno.Web.Areas.Admin.Configuration.Themes.Models;
using Inferno.Web.Configuration;
using Inferno.Web.Configuration.Services;
using Inferno.Web.Mvc.Themes;
using Inferno.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Inferno.Web.Areas.Admin.Configuration.Themes.Controllers.Api
{
    [Authorize]
    public class ThemeApiController : ODataController
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IThemeProvider themeProvider;
        private readonly IWorkContext workContext;
        private readonly SiteSettings siteSettings;
        private readonly ISettingService settingService;

        public ThemeApiController(
            IAuthorizationService authorizationService,
            IThemeProvider themeProvider,
            IWorkContext workContext,
            SiteSettings siteSettings,
            ISettingService settingService)
        {
            this.authorizationService = authorizationService;
            this.themeProvider = themeProvider;
            this.workContext = workContext;
            this.siteSettings = siteSettings;
            this.settingService = settingService;
        }

        public virtual async Task<IActionResult> Get(ODataQueryOptions<EdmThemeConfiguration> options)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.ThemesRead))
            {
                return Unauthorized();
            }

            var themes = themeProvider.GetThemeConfigurations()
                .Select(x => (EdmThemeConfiguration)x)
                .ToList();

            foreach (var theme in themes)
            {
                if (theme.Title == siteSettings.DefaultTheme)
                {
                    theme.IsDefaultTheme = true;
                }
            }

            return Ok(options.ApplyTo(themes.AsQueryable()));
        }

        [HttpPost]
        public virtual async Task<IActionResult> SetTheme([FromBody] ODataActionParameters parameters)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.ThemesWrite))
            {
                return Unauthorized();
            }

            if (parameters == null || !parameters.TryGetValue("themeName", out object themeNameObj))
            {
                return BadRequest("Missing required parameter 'themeName'.");
            }

            string themeName = themeNameObj as string;
            if (string.IsNullOrEmpty(themeName))
            {
                return BadRequest("'themeName' must be a non-empty string.");
            }

            var themeConfig = themeProvider.GetThemeConfiguration(themeName);
            if (themeConfig == null)
            {
                return NotFound();
            }

            siteSettings.DefaultTheme = themeName;
            siteSettings.DefaultFrontendLayoutPath = !string.IsNullOrEmpty(themeConfig.DefaultLayoutPath)
                ? themeConfig.DefaultLayoutPath
                : "~/Views/Shared/_Layout.cshtml";

            int tenantId = workContext.CurrentTenant?.Id ?? 0;
            settingService.SaveSettings(siteSettings, tenantId);

            return Ok();
        }

        protected virtual async Task<bool> AuthorizeAsync(string policyName)
        {
            if (authorizationService == null || string.IsNullOrEmpty(policyName))
            {
                return true;
            }

            return (await authorizationService.AuthorizeAsync(User, policyName)).Succeeded;
        }
    }
}
