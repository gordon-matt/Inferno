using Inferno.Security;

namespace Inferno.Web.Security.Permissions;

public sealed class InfernoWebPermissionProvider : IPermissionProvider
{
    private const string General = "General";
    private const string Localization = "Localization";
    private const string Maintenance = "Maintenance";
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
            Description = "Every permission, including tenants and any added by plugins later.",
        },

        new(InfernoWebPolicies.LanguagesRead, Localization, "View languages"),
        new(InfernoWebPolicies.LanguagesWrite, Localization, "Manage languages") { Implies = [InfernoWebPolicies.LanguagesRead] },
        new(InfernoWebPolicies.LocalizableStringsRead, Localization, "View translations"),
        new(InfernoWebPolicies.LocalizableStringsWrite, Localization, "Edit translations") { Implies = [InfernoWebPolicies.LocalizableStringsRead] },

        new(InfernoWebPolicies.LogRead, Maintenance, "View the log"),
        new(InfernoWebPolicies.LogWrite, Maintenance, "Clear the log") { Implies = [InfernoWebPolicies.LogRead] },
        new(InfernoWebPolicies.ScheduledTasksRead, Maintenance, "View scheduled tasks"),
        new(InfernoWebPolicies.ScheduledTasksWrite, Maintenance, "Manage scheduled tasks") { Implies = [InfernoWebPolicies.ScheduledTasksRead] },

        new(InfernoWebPolicies.MembershipManage, Membership, "Manage users and roles")
        {
            Description = "Create, edit and delete users and roles, including who is in the Administrators role.",
        },
        new(InfernoWebPolicies.MembershipPermissionsWrite, Membership, "Assign permissions")
        {
            Description = "Grant and revoke permissions. Only permissions the user holds can be granted.",
            Implies = [InfernoWebPolicies.MembershipManage],
        },

        new(InfernoWebPolicies.PluginsRead, Configuration, "View plugins"),
        new(InfernoWebPolicies.PluginsManage, Configuration, "Install and remove plugins") { Implies = [InfernoWebPolicies.PluginsRead] },
        new(InfernoWebPolicies.SettingsRead, Configuration, "View settings"),
        new(InfernoWebPolicies.SettingsWrite, Configuration, "Edit settings") { Implies = [InfernoWebPolicies.SettingsRead] },
        new(InfernoWebPolicies.ThemesRead, Configuration, "View themes"),
        new(InfernoWebPolicies.ThemesWrite, Configuration, "Change the theme") { Implies = [InfernoWebPolicies.ThemesRead] },
    ];
}
