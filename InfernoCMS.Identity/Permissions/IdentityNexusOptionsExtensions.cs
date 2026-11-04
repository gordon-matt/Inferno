using IdentityNexus.Extensibility;
using IdentityNexus.Services;
using Inferno.Web.Security.Permissions;

namespace InfernoCMS.Identity.Permissions;

public static class IdentityNexusOptionsExtensions
{
    /// <summary>Adds the Permissions tab to the user and role editors, and keeps permission claims out of the generic claim list.</summary>
    public static IdentityNexusOptions AddPermissionSections(this IdentityNexusOptions options) => options
        .AddUserSection<PermissionsSection>("Permissions", Permission.ClaimType)
        .AddRoleSection<PermissionsSection>("Permissions", Permission.ClaimType);
}
