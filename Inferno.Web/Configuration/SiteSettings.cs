using Localize;
using Inferno.Web.Areas.Admin.Configuration.Components;

namespace Inferno.Web.Configuration;

/// <summary>The tenant's site name, blog switch, language, and default meta tags.</summary>
public class SiteSettings : ISettings
{
    /// <summary>Sets the site name, a grid page size of 10, the blog on, and the home page title.</summary>
    public SiteSettings()
    {
        SiteName = "My Site";
        DefaultGridPageSize = 10;
        EnableBlog = true;
        ShowLanguageSwitcher = true;
        //AdminLayoutPath = "~/Areas/Admin/Views/Shared/_Layout.cshtml";
        HomePageTitle = "Home Page";
    }

    #region ISettings Members

    /// <inheritdoc />
    public string Name => "Site Settings";

    /// <inheritdoc />
    public bool IsTenantRestricted => false;

    /// <inheritdoc />
    public Type EditorType => typeof(SiteSettingsEditor);

    #endregion ISettings Members

    #region General

    /// <summary>The name shown for the site. Defaults to "My Site".</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.SiteName)]
    public string SiteName { get; set; }

    //[LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.AdminLayoutPath)]
    //public string AdminLayoutPath { get; set; }

    /// <summary>How many rows an admin grid shows by default. Defaults to 10.</summary>
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

    /// <summary>The culture code used when a visitor has not chosen one.</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.DefaultLanguage)]
    public string DefaultLanguage { get; set; }

    /// <summary>Whether the public menu shows the language switch. Each tenant chooses for its own site.</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.ShowLanguageSwitcher)]
    public bool ShowLanguageSwitcher { get; set; }

    #endregion Localization

    #region SEO

    /// <summary>Meta keywords written when a page does not set its own.</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.DefaultMetaKeywords)]
    public string DefaultMetaKeywords { get; set; }

    /// <summary>Meta description written when a page does not set its own.</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.DefaultMetaDescription)]
    public string DefaultMetaDescription { get; set; }

    /// <summary>The title of the home page. Defaults to "Home Page".</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Site.HomePageTitle)]
    public string HomePageTitle { get; set; }

    #endregion SEO
}