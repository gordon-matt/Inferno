using IdentityNexus.Extensibility;
using IdentityNexus.Services;
using Inferno.Web.Security.Permissions;

namespace InfernoCMS.Identity.Permissions;

/// <summary>Adds Inferno permission editing to IdentityNexus.</summary>
public static class IdentityNexusOptionsExtensions
{
    /// <summary>Adds the Permissions tab to the user and role editors, and keeps permission claims out of the generic claim list.</summary>
    /// <param name="options">The IdentityNexus options being configured.</param>
    /// <returns>The same <paramref name="options"/>, so further configuration can be chained.</returns>
    public static IdentityNexusOptions AddPermissionSections(this IdentityNexusOptions options) => options
        .AddUserSection<PermissionsSection>("Permissions", Permission.ClaimType)
        .AddRoleSection<PermissionsSection>("Permissions", Permission.ClaimType);
}
