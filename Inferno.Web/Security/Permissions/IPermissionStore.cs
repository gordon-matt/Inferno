using Ardalis.Result;

namespace Inferno.Web.Security.Permissions;

/// <summary>
/// Reads and writes permission grants. Grants are claims of type <see cref="Permission.ClaimType"/> on Identity roles and users,
/// and they are read from the database rather than the sign-in cookie, so a change applies on the user's next check.
/// </summary>
public interface IPermissionStore
{
    /// <summary>Everything <paramref name="userId"/> holds. Cached briefly; changes made through this store clear the cache.</summary>
    /// <param name="userId">The Identity user id. Empty returns <see cref="GrantedPermissions.None"/> without a database read.</param>
    /// <param name="cancellationToken">Observed by the database reads. A cached result does not use it.</param>
    /// <returns>The grants, or <see cref="GrantedPermissions.None"/> when <paramref name="userId"/> is empty or the read fails.</returns>
    Task<GrantedPermissions> GetGrantedAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Permissions granted to the user directly, not through a role.</summary>
    /// <param name="userId">The Identity user id.</param>
    /// <param name="cancellationToken">Observed by the database read.</param>
    /// <returns>Permission names stored on the user. Empty values are omitted.</returns>
    Task<IReadOnlySet<string>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>The user's roles and what each one grants.</summary>
    /// <param name="userId">The Identity user id.</param>
    /// <param name="cancellationToken">Observed by the database reads.</param>
    /// <returns>One entry per role, ordered by role name.</returns>
    Task<IReadOnlyList<RolePermissions>> GetUserRolePermissionsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Permission names granted to the role.</summary>
    /// <param name="roleId">The Identity role id.</param>
    /// <param name="cancellationToken">Observed by the database read.</param>
    /// <returns>The role's permission names. Empty values are omitted.</returns>
    Task<IReadOnlySet<string>> GetRolePermissionsAsync(string roleId, CancellationToken cancellationToken = default);

    /// <summary>Replaces the user's direct grants with <paramref name="permissions"/>.</summary>
    /// <param name="userId">The Identity user id.</param>
    /// <param name="permissions">The grants that should remain. Blank values are dropped. <see langword="null"/> clears them.</param>
    /// <param name="cancellationToken">Observed by the database reads and the save.</param>
    /// <returns>Success, or not found when the user does not exist.</returns>
    Task<Result> SetUserPermissionsAsync(string userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);

    /// <summary>Replaces the role's grants with <paramref name="permissions"/>. Cached grants are cleared because the change reaches every member.</summary>
    /// <param name="roleId">The Identity role id.</param>
    /// <param name="permissions">The grants that should remain. Blank values are dropped. <see langword="null"/> clears them.</param>
    /// <param name="cancellationToken">Observed by the database reads and the save.</param>
    /// <returns>Success, or not found when the role does not exist.</returns>
    Task<Result> SetRolePermissionsAsync(string roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);

    /// <summary>The grants already cached for <paramref name="userId"/>. Does not read the database, so it is safe to call while rendering.</summary>
    /// <param name="userId">The Identity user id. Empty never hits the cache.</param>
    /// <param name="granted">The cached grants when this returns <see langword="true"/>; otherwise <see cref="GrantedPermissions.None"/>.</param>
    /// <returns><see langword="true"/> when a cache entry exists.</returns>
    bool TryGetCached(string userId, out GrantedPermissions granted);

    /// <summary>Forgets the cached grants of one user, for a change made outside this store.</summary>
    /// <param name="userId">The Identity user id. Empty does nothing.</param>
    void Invalidate(string userId);

    /// <summary>Forgets every cached grant, for a role change that reaches many users.</summary>
    void InvalidateAll();
}
