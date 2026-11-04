namespace Inferno.Web.Security.Permissions;

/// <summary>What a user holds right now: their own grants plus those of their roles.</summary>
/// <param name="IsAdministrator">In an Administrators role, which holds every permission of its tenant.</param>
/// <param name="IsHostAdministrator">In the Administrators role that belongs to no tenant, which also holds <see cref="Inferno.Security.StandardPolicies.FullAccess"/>.</param>
/// <param name="Permissions">Permission names granted directly or through a role, before implied permissions are added.</param>
public sealed record GrantedPermissions(bool IsAdministrator, bool IsHostAdministrator, IReadOnlySet<string> Permissions)
{
    public static GrantedPermissions None { get; } = new(false, false, new HashSet<string>(StringComparer.Ordinal));
}

/// <summary>The permissions one role gives its members.</summary>
public sealed record RolePermissions(string RoleId, string RoleName, IReadOnlySet<string> Permissions);
