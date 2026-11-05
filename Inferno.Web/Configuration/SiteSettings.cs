using Localize;
using Inferno.Web.Areas.Admin.Configuration.Components;

namespace Inferno.Web.Configuration;

public class SiteSettings : ISettings
{
    public SiteSettings()
    {
        SiteName = "My Site";
        DefaultGridPageSize = 10;
        EnableBlog = true;
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

    //[LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.AdminLayoutPath)]
    //public string AdminLayoutPath { get; set; }

    [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.DefaultGridPageSize)]
    public int DefaultGridPageSize { get; set; }

    /// <summary>Whether this tenant has a blog. When off, the Blog link is left out of the menu and /blog answers 404.</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.EnableBlog)]
    public bool EnableBlog { get; set; }

    /// <summary>Whether visitors may create their own account. Off by default: accounts are made by an administrator. A new account has no permissions either way.</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.AllowRegistration)]
    public bool AllowRegistration { get; set; }

    #endregion General

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