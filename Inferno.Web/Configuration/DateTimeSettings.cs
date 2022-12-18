using System.ComponentModel.DataAnnotations;
using Inferno.Localization.ComponentModel;
using Inferno.Web.Areas.Admin.Configuration.Components;

namespace Inferno.Web.Configuration
{
    public class DateTimeSettings : ISettings
    {
        [Required]
        [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.DateTime.DefaultTimeZoneId)]
        public string DefaultTimeZoneId { get; set; }

        [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.DateTime.AllowUsersToSetTimeZone)]
        public bool AllowUsersToSetTimeZone { get; set; }

        #region ISettings Members

        public string Name => "Date/Time Settings";

        public bool IsTenantRestricted => false;

        public Type EditorType => typeof(DateTimeSettingsEditor);

        //public string EditorTemplatePath => "Inferno.Web.Views.Shared.EditorTemplates.DateTimeSettings.cshtml";

        #endregion ISettings Members
    }
}