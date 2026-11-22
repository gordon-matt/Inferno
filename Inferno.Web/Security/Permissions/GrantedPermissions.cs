using Inferno.Security;

namespace Inferno.Web.Security.Permissions;

/// <summary>What a user holds right now: their own grants plus those of their roles.</summary>
/// <param name="IsAdministrator">In an Administrators role, which holds every permission of its tenant.</param>
/// <param name="IsHostAdministrator">In the Administrators role that belongs to no tenant, which also holds <see cref="Inferno.Security.StandardPolicies.FullAccess"/>.</param>
/// <param name="Permissions">Permission names granted directly or through a role, before implied permissions are added.</param>
public sealed record GrantedPermissions(bool IsAdministrator, bool IsHostAdministrator, IReadOnlySet<string> Permissions)
{
    /// <summary>No grants and neither administrator flag.</summary>
    public static GrantedPermissions None { get; } = new(false, false, new HashSet<string>(StringComparer.Ordinal));

    /// <summary>
    /// The same grants a tenant administrator holds. Host-wide access is removed.
    /// A caller who is not a host administrator is returned unchanged.
    /// </summary>
    /// <returns>The tenant-administrator view of these grants.</returns>
    public GrantedPermissions AsTenantAdministrator()
    {
        if (!IsHostAdministrator)
        {
            return this;
        }

        var permissions = Permissions
            .Where(name => name != StandardPolicies.FullAccess)
            .ToHashSet(StringComparer.Ordinal);
        return new GrantedPermissions(IsAdministrator: true, IsHostAdministrator: false, permissions);
    }
}

/// <summary>The permissions one role gives its members.</summary>
/// <param name="RoleId">The Identity role id.</param>
/// <param name="RoleName">The role name.</param>
/// <param name="Permissions">Permission names stored on the role.</param>
public sealed record RolePermissions(string RoleId, string RoleName, IReadOnlySet<string> Permissions);
