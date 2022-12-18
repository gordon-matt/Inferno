using Inferno.Localization.ComponentModel;
using Inferno.Web.Areas.Admin.Configuration.Components;

namespace Inferno.Web.Configuration
{
    public class SiteSettings : ISettings
    {
        public SiteSettings()
        {
            SiteName = "My Site";
            //DefaultTheme = "Default";
            DefaultGridPageSize = 10;
            //DefaultFrontendLayoutPath = "~/Views/Shared/_Layout.cshtml";
            //AdminLayoutPath = "~/Areas/Admin/Views/Shared/_Layout.cshtml";
            HomePageTitle = "Home Page";
        }

        #region ISettings Members

        public string Name => "Site Settings";

        public bool IsTenantRestricted => false;

        public Type EditorType => typeof(SiteSettingsEditor);

        #endregion ISettings Members

        #region General

        [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.SiteName)]
        public string SiteName { get; set; }

        //[LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.DefaultFrontendLayoutPath)]
        //public string DefaultFrontendLayoutPath { get; set; }

        //[LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.AdminLayoutPath)]
        //public string AdminLayoutPath { get; set; }

        [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.DefaultGridPageSize)]
        public int DefaultGridPageSize { get; set; }

        #endregion General

        //#region Themes

        //[LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.DefaultTheme)]
        //public string DefaultTheme { get; set; }

        //[LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.AllowUserToSelectTheme)]
        //public bool AllowUserToSelectTheme { get; set; }

        //#endregion Themes

        #region Localization

        [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.DefaultLanguage)]
        public string DefaultLanguage { get; set; }

        #endregion Localization

        #region SEO

        [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.DefaultMetaKeywords)]
        public string DefaultMetaKeywords { get; set; }

        [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.DefaultMetaDescription)]
        public string DefaultMetaDescription { get; set; }

        [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.HomePageTitle)]
        public string HomePageTitle { get; set; }

        #endregion SEO
    }
}