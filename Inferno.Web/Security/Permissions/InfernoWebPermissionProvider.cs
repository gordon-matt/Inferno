using Inferno.Security;

namespace Inferno.Web.Security.Permissions;

public sealed class InfernoWebPermissionProvider : IPermissionProvider
{
    private const string General = "General";
    private const string Localization = "Localization";
    private const string Membership = "Membership";
    private const string Configuration = "Configuration";

    public IEnumerable<Permission> GetPermissions() =>
    [
        new(StandardPolicies.AdminAccess, General, "Admin area")
        {
            Description = "Open the admin dashboard. Any other admin permission includes this one.",
        },
        new(StandardPolicies.FullAccess, General, "Full access")
        {
            Description = "Every permission, plus what belongs to the whole application: tenants, plugins, the log, scheduled tasks and the theme catalog. Only the host administrator has it.",
        },

        new(InfernoWebPolicies.LanguagesRead, Localization, "View languages"),
        new(InfernoWebPolicies.LanguagesWrite, Localization, "Manage languages") { Implies = [InfernoWebPolicies.LanguagesRead] },
        new(InfernoWebPolicies.LocalizableStringsRead, Localization, "View translations"),
        new(InfernoWebPolicies.LocalizableStringsWrite, Localization, "Edit translations") { Implies = [InfernoWebPolicies.LocalizableStringsRead] },

        new(InfernoWebPolicies.MembershipManage, Membership, "Manage users and roles")
        {
            Description = "Create, edit and delete users and roles, including who is in the Administrators role.",
        },
        new(InfernoWebPolicies.MembershipPermissionsWrite, Membership, "Assign permissions")
        {
            Description = "Grant and revoke permissions. Only permissions the user holds can be granted.",
            Implies = [InfernoWebPolicies.MembershipManage],
        },

        new(InfernoWebPolicies.SettingsRead, Configuration, "View settings"),
        new(InfernoWebPolicies.SettingsWrite, Configuration, "Edit settings") { Implies = [InfernoWebPolicies.SettingsRead] },
        new(InfernoWebPolicies.ThemesRead, Configuration, "View themes"),
        new(InfernoWebPolicies.ThemesWrite, Configuration, "Change the theme") { Implies = [InfernoWebPolicies.ThemesRead] },
    ];
}
