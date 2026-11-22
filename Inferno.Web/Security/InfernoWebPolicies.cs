namespace Inferno.Web.Security;

/// <summary>
/// The core permissions. Each value is both a policy name and a stored claim value, so renaming one
/// orphans existing grants. <see cref="Permissions.InfernoWebPermissionProvider"/> describes them.
/// </summary>
public static class InfernoWebPolicies
{
    // Localization
    /// <summary>View the language list.</summary>
    public const string LanguagesRead = "Inferno.Languages.Read";

    /// <summary>Create and change languages. Implies <see cref="LanguagesRead"/>.</summary>
    public const string LanguagesWrite = "Inferno.Languages.Write";

    /// <summary>View translations.</summary>
    public const string LocalizableStringsRead = "Inferno.LocalizableStrings.Read";

    /// <summary>Edit translations. Implies <see cref="LocalizableStringsRead"/>.</summary>
    public const string LocalizableStringsWrite = "Inferno.LocalizableStrings.Write";

    // Settings
    /// <summary>View settings.</summary>
    public const string SettingsRead = "Inferno.Settings.Read";

    /// <summary>Edit settings. Implies <see cref="SettingsRead"/>.</summary>
    public const string SettingsWrite = "Inferno.Settings.Write";

    // Themes
    /// <summary>View themes.</summary>
    public const string ThemesRead = "Inferno.Themes.Read";

    /// <summary>Change the current tenant's theme. Implies <see cref="ThemesRead"/>.</summary>
    public const string ThemesWrite = "Inferno.Themes.Write";

    // Membership
    /// <summary>Create, edit, and delete users and roles, including membership of the Administrators role.</summary>
    public const string MembershipManage = "Inferno.Membership.Manage";

    /// <summary>Grant and revoke permissions the user holds. Implies <see cref="MembershipManage"/>.</summary>
    public const string MembershipPermissionsWrite = "Inferno.Membership.Permissions.Write";
}
