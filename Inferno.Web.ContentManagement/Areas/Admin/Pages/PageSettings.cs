using Inferno.Localization.ComponentModel;
using Inferno.Web.Configuration;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages
{
    public class PageSettings : ISettings
    {
        public PageSettings()
        {
            NumberOfPageVersionsToKeep = 5;
        }

        #region ISettings Members

        public string Name => "CMS: Page Settings";

        public bool IsTenantRestricted => false;

        public Type EditorType => typeof(PageSettingsEditor);

        #endregion ISettings Members

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.Settings.Pages.NumberOfPageVersionsToKeep)]
        public short NumberOfPageVersionsToKeep { get; set; }
    }
}