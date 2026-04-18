using Inferno.Security;
using Microsoft.AspNetCore.Authorization;

namespace Inferno.Web.Security
{
    public static class AuthorizationOptionsExtensions
    {
        /// <summary>
        /// The claim type used by Inferno to carry fine-grained permission values (e.g. "SettingsRead").
        /// </summary>
        public const string PermissionClaimType = "Permission";

        public static void AddInfernoWebPolicies(this AuthorizationOptions options)
        {
            options.AddPermission(InfernoWebPolicies.LanguagesRead, "LanguagesRead");
            options.AddPermission(InfernoWebPolicies.LanguagesWrite, "LanguagesWrite");
            options.AddPermission(InfernoWebPolicies.LocalizableStringsRead, "LocalizableStringsRead");
            options.AddPermission(InfernoWebPolicies.LocalizableStringsWrite, "LocalizableStringsWrite");
            options.AddPermission(InfernoWebPolicies.LogRead, "LogRead");
            options.AddPermission(InfernoWebPolicies.LogWrite, "LogWrite");
            options.AddPermission(InfernoWebPolicies.ScheduledTasksRead, "ScheduledTasksRead");
            options.AddPermission(InfernoWebPolicies.ScheduledTasksWrite, "ScheduledTasksWrite");
            options.AddPermission(InfernoWebPolicies.SettingsRead, "SettingsRead");
            options.AddPermission(InfernoWebPolicies.SettingsWrite, "SettingsWrite");
            options.AddPermission(InfernoWebPolicies.ThemesRead, "ThemesRead");
            options.AddPermission(InfernoWebPolicies.ThemesWrite, "ThemesWrite");
            options.AddPermission(InfernoWebPolicies.MembershipManage, "MembershipManage");
            options.AddPermission(InfernoWebPolicies.MembershipPermissionsRead, "MembershipPermissionsRead");
            options.AddPermission(InfernoWebPolicies.MembershipPermissionsWrite, "MembershipPermissionsWrite");
            options.AddPermission(InfernoWebPolicies.MembershipRolesRead, "MembershipRolesRead");
            options.AddPermission(InfernoWebPolicies.MembershipRolesWrite, "MembershipRolesWrite");
            options.AddPermission(InfernoWebPolicies.MembershipUsersRead, "MembershipUsersRead");
            options.AddPermission(InfernoWebPolicies.MembershipUsersWrite, "MembershipUsersWrite");
        }

        /// <summary>
        /// Registers an authorization policy that is satisfied when the caller either
        /// <list type="bullet">
        ///   <item>has the <c>Administrators</c> role, or</item>
        ///   <item>has a <c>Permission</c> claim of <c>FullAccess</c> (super admin bypass), or</item>
        ///   <item>has a <c>Permission</c> claim matching <paramref name="claimValue"/>.</c></item>
        /// </list>
        /// This lets the built-in admin user (who has the <c>Administrators</c> role) use every
        /// admin feature, while still allowing granular <c>Permission</c> claims to be assigned
        /// to non-admin roles/users to unlock individual endpoints.
        /// </summary>
        public static void AddPermission(this AuthorizationOptions options, string policyName, string claimValue)
        {
            options.AddPolicy(policyName, policy => policy.RequireAssertion(ctx =>
                ctx.User.IsInRole(InfernoSecurityConstants.Roles.Administrators) ||
                ctx.User.HasClaim(PermissionClaimType, StandardPolicies.FullAccess) ||
                ctx.User.HasClaim(PermissionClaimType, claimValue)));
        }
    }
}