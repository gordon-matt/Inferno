using Microsoft.AspNetCore.Authorization;

namespace Inferno.Web.Security
{
    public static class AuthorizationOptionsExtensions
    {
        public static void AddInfernoWebPolicies(this AuthorizationOptions options)
        {
            options.AddPolicy(InfernoWebPolicies.LanguagesRead, policy => policy.RequireClaim("Permission", "LanguagesRead"));
            options.AddPolicy(InfernoWebPolicies.LanguagesWrite, policy => policy.RequireClaim("Permission", "LanguagesWrite"));
            options.AddPolicy(InfernoWebPolicies.LocalizableStringsRead, policy => policy.RequireClaim("Permission", "LocalizableStringsRead"));
            options.AddPolicy(InfernoWebPolicies.LocalizableStringsWrite, policy => policy.RequireClaim("Permission", "LocalizableStringsWrite"));
            options.AddPolicy(InfernoWebPolicies.LogRead, policy => policy.RequireClaim("Permission", "LogRead"));
            options.AddPolicy(InfernoWebPolicies.ScheduledTasksRead, policy => policy.RequireClaim("Permission", "ScheduledTasksRead"));
            options.AddPolicy(InfernoWebPolicies.ScheduledTasksWrite, policy => policy.RequireClaim("Permission", "ScheduledTasksWrite"));
            options.AddPolicy(InfernoWebPolicies.SettingsRead, policy => policy.RequireClaim("Permission", "SettingsRead"));
            options.AddPolicy(InfernoWebPolicies.SettingsWrite, policy => policy.RequireClaim("Permission", "SettingsWrite"));
            options.AddPolicy(InfernoWebPolicies.MembershipManage, policy => policy.RequireClaim("Permission", "MembershipManage"));
            options.AddPolicy(InfernoWebPolicies.MembershipPermissionsRead, policy => policy.RequireClaim("Permission", "MembershipPermissionsRead"));
            options.AddPolicy(InfernoWebPolicies.MembershipPermissionsWrite, policy => policy.RequireClaim("Permission", "MembershipPermissionsWrite"));
            options.AddPolicy(InfernoWebPolicies.MembershipRolesRead, policy => policy.RequireClaim("Permission", "MembershipRolesRead"));
            options.AddPolicy(InfernoWebPolicies.MembershipRolesWrite, policy => policy.RequireClaim("Permission", "MembershipRolesWrite"));
            options.AddPolicy(InfernoWebPolicies.MembershipUsersRead, policy => policy.RequireClaim("Permission", "MembershipUsersRead"));
            options.AddPolicy(InfernoWebPolicies.MembershipUsersWrite, policy => policy.RequireClaim("Permission", "MembershipUsersWrite"));
        }
    }
}