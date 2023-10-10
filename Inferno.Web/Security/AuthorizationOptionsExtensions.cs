using Microsoft.AspNetCore.Authorization;

namespace Inferno.Web.Security
{
    public static class AuthorizationOptionsExtensions
    {
        public static void AddInfernoWebPolicies(this AuthorizationOptions options)
        {
            options.AddPermission(InfernoWebPolicies.LanguagesRead, "LanguagesRead");
            options.AddPermission(InfernoWebPolicies.LanguagesWrite, "LanguagesWrite");
            options.AddPermission(InfernoWebPolicies.LocalizableStringsRead, "LocalizableStringsRead");
            options.AddPermission(InfernoWebPolicies.LocalizableStringsWrite, "LocalizableStringsWrite");
            options.AddPermission(InfernoWebPolicies.LogRead, "LogRead");
            options.AddPermission(InfernoWebPolicies.ScheduledTasksRead, "ScheduledTasksRead");
            options.AddPermission(InfernoWebPolicies.ScheduledTasksWrite, "ScheduledTasksWrite");
            options.AddPermission(InfernoWebPolicies.SettingsRead, "SettingsRead");
            options.AddPermission(InfernoWebPolicies.SettingsWrite, "SettingsWrite");
            options.AddPermission(InfernoWebPolicies.MembershipManage, "MembershipManage");
            options.AddPermission(InfernoWebPolicies.MembershipPermissionsRead, "MembershipPermissionsRead");
            options.AddPermission(InfernoWebPolicies.MembershipPermissionsWrite, "MembershipPermissionsWrite");
            options.AddPermission(InfernoWebPolicies.MembershipRolesRead, "MembershipRolesRead");
            options.AddPermission(InfernoWebPolicies.MembershipRolesWrite, "MembershipRolesWrite");
            options.AddPermission(InfernoWebPolicies.MembershipUsersRead, "MembershipUsersRead");
            options.AddPermission(InfernoWebPolicies.MembershipUsersWrite, "MembershipUsersWrite");
        }

        public static void AddPermission(this AuthorizationOptions options, string policyName, string claimValue)
        {
            options.AddPolicy(policyName, policy => policy.RequireClaim("Permission", claimValue));
        }
    }
}