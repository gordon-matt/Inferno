using Inferno.Localization.ComponentModel;
using Inferno.Web.Configuration;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog
{
    public class BlogSettings : ISettings
    {
        public BlogSettings()
        {
            DateFormat = "YYYY-MM-DD HH:mm:ss";
            ItemsPerPage = 5;
            PageTitle = "Blog";
            ShowOnMenus = true;
            MenuPosition = 0;
        }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.Settings.Blog.PageTitle)]
        public string PageTitle { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.Settings.Blog.DateFormat)]
        public string DateFormat { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.Settings.Blog.ItemsPerPage)]
        public byte ItemsPerPage { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.Settings.Blog.ShowOnMenus)]
        public bool ShowOnMenus { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.Settings.Blog.MenuPosition)]
        public byte MenuPosition { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.Settings.Blog.RoleIds)]
        public IEnumerable<string> RoleIds { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.Settings.Blog.LayoutPathOverride)]
        public string LayoutPathOverride { get; set; }

        #region ISettings Members

        public string Name => "CMS: Blog Settings";

        public bool IsTenantRestricted => false;

        public Type EditorType => typeof(BlogSettingsEditor);

        #endregion ISettings Members
    }
}