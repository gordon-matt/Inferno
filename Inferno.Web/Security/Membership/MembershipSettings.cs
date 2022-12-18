using Inferno.Localization.ComponentModel;
using Inferno.Web.Areas.Admin.Configuration.Components;
using Inferno.Web.Configuration;

namespace Inferno.Web.Security.Membership
{
    public class MembershipSettings : ISettings
    {
        public MembershipSettings()
        {
            GeneratedPasswordLength = 7;
            GeneratedPasswordNumberOfNonAlphanumericChars = 3;
        }

        [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Membership.GeneratedPasswordLength)]
        public byte GeneratedPasswordLength { get; set; }

        [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Membership.GeneratedPasswordNumberOfNonAlphanumericChars)]
        public byte GeneratedPasswordNumberOfNonAlphanumericChars { get; set; }

        [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Membership.DisallowUnconfirmedUserLogin)]
        public bool DisallowUnconfirmedUserLogin { get; set; }

        #region ISettings Members

        public string Name => "Membership Settings";

        public bool IsTenantRestricted => false;

        public Type EditorType => typeof(MembershipSettingsEditor);

        //public string EditorTemplatePath => "Inferno.Web.Views.Shared.EditorTemplates.MembershipSettings.cshtml";

        #endregion ISettings Members
    }
}