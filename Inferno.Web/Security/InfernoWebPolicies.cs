namespace Inferno.Web.Security;

/// <summary>
/// The core permissions. Each value is both a policy name and a stored claim value, so renaming one
/// orphans existing grants. <see cref="Permissions.InfernoWebPermissionProvider"/> describes them.
/// </summary>
public static class InfernoWebPolicies
{
    // Localization
    public const string LanguagesRead = "Inferno.Languages.Read";
    public const string LanguagesWrite = "Inferno.Languages.Write";
    public const string LocalizableStringsRead = "Inferno.LocalizableStrings.Read";
    public const string LocalizableStringsWrite = "Inferno.LocalizableStrings.Write";

    // Settings
    public const string SettingsRead = "Inferno.Settings.Read";
    public const string SettingsWrite = "Inferno.Settings.Write";

    // Themes
    public const string ThemesRead = "Inferno.Themes.Read";
    public const string ThemesWrite = "Inferno.Themes.Write";

    // Membership
    public const string MembershipManage = "Inferno.Membership.Manage";
    public const string MembershipPermissionsWrite = "Inferno.Membership.Permissions.Write";
}
