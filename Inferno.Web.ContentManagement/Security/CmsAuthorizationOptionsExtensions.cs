using Inferno.Web.Security.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace Inferno.Web.ContentManagement.Security;

/// <summary>
/// The CMS permissions need no policies of their own, because <see cref="PermissionPolicyProvider"/> builds them.
/// These are policies named by other libraries that the CMS answers with one of its permissions.
/// </summary>
public static class CmsAuthorizationOptionsExtensions
{
    public static void AddInfernoCmsPolicies(this AuthorizationOptions options) =>
        // BlogKrafter names its editor policy BlogKrafter.Manage. Defining it here makes the blog write
        // permission satisfy it, and stops BlogKrafter from adding a role-based policy of the same name.
        options.AddPolicy("BlogKrafter.Manage", policy => policy
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(CmsConstants.Policies.BlogWrite)));
}
