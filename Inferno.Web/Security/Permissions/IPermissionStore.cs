using Ardalis.Result;

namespace Inferno.Web.Security.Permissions;

/// <summary>
/// Reads and writes permission grants. Grants are claims of type <see cref="Permission.ClaimType"/> on Identity roles and users,
/// and they are read from the database rather than the sign-in cookie, so a change applies on the user's next check.
/// </summary>
public interface IPermissionStore
{
    /// <summary>Everything <paramref name="userId"/> holds. Cached briefly; changes made through this store clear the cache.</summary>
    Task<GrantedPermissions> GetGrantedAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Permissions granted to the user directly, not through a role.</summary>
    Task<IReadOnlySet<string>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>The user's roles and what each one grants.</summary>
    Task<IReadOnlyList<RolePermissions>> GetUserRolePermissionsAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlySet<string>> GetRolePermissionsAsync(string roleId, CancellationToken cancellationToken = default);

    /// <summary>Replaces the user's direct grants with <paramref name="permissions"/>.</summary>
    Task<Result> SetUserPermissionsAsync(string userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);

    /// <summary>Replaces the role's grants with <paramref name="permissions"/>.</summary>
    Task<Result> SetRolePermissionsAsync(string roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);

    /// <summary>The grants already cached for <paramref name="userId"/>. Does not read the database, so it is safe to call while rendering.</summary>
    bool TryGetCached(string userId, out GrantedPermissions granted);

    /// <summary>Forgets the cached grants of one user, for a change made outside this store.</summary>
    void Invalidate(string userId);

    /// <summary>Forgets every cached grant, for a role change that reaches many users.</summary>
    void InvalidateAll();
}
