using Inferno.Security;
using Inferno.Web.Security.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace Inferno.Web.Security;

/// <summary>
/// The core permissions need no policies of their own, because <see cref="PermissionPolicyProvider"/> builds them.
/// These are policies named by other libraries that a core permission answers.
/// </summary>
public static class InfernoWebAuthorizationOptionsExtensions
{
    /// <summary>Kameleon's policy for opening a site's theme screen. Reading themes is enough.</summary>
    public const string ThemeView = "Kameleon.View";

    /// <summary>Kameleon's policy for changing the current tenant's theme settings.</summary>
    public const string ThemeManage = "Kameleon.Manage";

    /// <summary>Kameleon's policy for the application-wide catalog: themes on or off for everyone and per tenant.</summary>
    public const string ThemeManageCatalog = "Kameleon.ManageCatalog";

    /// <remarks>
    /// Defining the policies here, rather than leaving Kameleon's role-based ones, makes the theme permissions
    /// grantable like every other permission. The catalog changes what every tenant can use, so it needs full
    /// access, which only the host administrator holds. The names are repeated as text because this project does
    /// not reference Kameleon; the host's Kameleon setup is the only place that does.
    /// </remarks>
    public static void AddInfernoWebPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(ThemeView, policy => policy
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(InfernoWebPolicies.ThemesRead)));

        options.AddPolicy(ThemeManage, policy => policy
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(InfernoWebPolicies.ThemesWrite)));

        options.AddPolicy(ThemeManageCatalog, policy => policy
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(StandardPolicies.FullAccess)));
    }
}
