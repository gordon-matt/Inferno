using Localize;
using Inferno.Web.Areas.Admin.Configuration.Components;
using Inferno.Web.Configuration;

namespace Inferno.Web.Security.Membership;

/// <summary>Password generation and whether an unconfirmed user may sign in.</summary>
public class MembershipSettings : ISettings
{
    /// <summary>Sets generated passwords to 7 characters, 3 of them non-alphanumeric.</summary>
    public MembershipSettings()
    {
        GeneratedPasswordLength = 7;
        GeneratedPasswordNumberOfNonAlphanumericChars = 3;
    }

    /// <summary>How many characters a generated password contains. Defaults to 7.</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Membership.GeneratedPasswordLength)]
    public byte GeneratedPasswordLength { get; set; }

    /// <summary>How many non-alphanumeric characters a generated password contains. Defaults to 3.</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Membership.GeneratedPasswordNumberOfNonAlphanumericChars)]
    public byte GeneratedPasswordNumberOfNonAlphanumericChars { get; set; }

    /// <summary>Whether a user whose email is not confirmed is refused at sign-in.</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.Membership.DisallowUnconfirmedUserLogin)]
    public bool DisallowUnconfirmedUserLogin { get; set; }

    #region ISettings Members

    /// <inheritdoc />
    public string Name => "Membership Settings";

    /// <inheritdoc />
    public bool IsTenantRestricted => false;

    /// <inheritdoc />
    public Type EditorType => typeof(MembershipSettingsEditor);

    //public string EditorTemplatePath => "Inferno.Web.Views.Shared.EditorTemplates.MembershipSettings.cshtml";

    #endregion ISettings Members
}