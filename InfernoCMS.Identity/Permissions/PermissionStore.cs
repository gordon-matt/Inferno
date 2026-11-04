using Ardalis.Result;
using Extenso.Data.Entity;
using Inferno.Security;
using Inferno.Web.Security.Permissions;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace InfernoCMS.Identity.Permissions;

/// <summary>
/// Grants live in the Identity role and user claim tables. Each check reads them through a short cache instead of
/// the sign-in cookie, so revoking a permission takes effect without the user signing out.
/// </summary>
public sealed class PermissionStore(
    IDbContextFactory contextFactory,
    IMemoryCache cache,
    ILogger<PermissionStore> logger) : IPermissionStore
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    // Bumping the generation retires every cached entry at once; the old ones expire on their own.
    private int generation;

    public async Task<GrantedPermissions> GetGrantedAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return GrantedPermissions.None;
        }

        if (cache.TryGetValue(CacheKey(userId), out GrantedPermissions cached))
        {
            return cached;
        }

        try
        {
            var roles = await GetUserRolePermissionsAsync(userId, cancellationToken);
            var direct = await GetUserPermissionsAsync(userId, cancellationToken);
            var administratorRoles = await AdministratorRolesAsync(roles.Select(role => role.RoleId), cancellationToken);

            var permissions = new HashSet<string>(direct, StringComparer.Ordinal);
            permissions.UnionWith(roles.SelectMany(role => role.Permissions));

            var granted = new GrantedPermissions(
                IsAdministrator: administratorRoles.Count > 0,
                IsHostAdministrator: administratorRoles.Any(role => role.TenantId is null),
                Permissions: permissions);

            cache.Set(CacheKey(userId), granted, CacheDuration);
            return granted;
        }
        catch (Exception exception)
        {
            // Fail closed: a user whose grants cannot be read holds nothing until they can.
            logger.LogError(exception, "Could not read the permissions of user {UserId}.", userId);
            return GrantedPermissions.None;
        }
    }

    public async Task<IReadOnlySet<string>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        await using var context = contextFactory.GetContext();
        var values = await context.Set<IdentityUserClaim<string>>()
            .Where(claim => claim.UserId == userId && claim.ClaimType == Permission.ClaimType)
            .Select(claim => claim.ClaimValue)
            .ToListAsync(cancellationToken);

        return values.Where(value => !string.IsNullOrEmpty(value)).ToHashSet(StringComparer.Ordinal);
    }

    public async Task<IReadOnlyList<RolePermissions>> GetUserRolePermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        await using var context = contextFactory.GetContext();
        var roles = await context.Set<IdentityUserRole<string>>()
            .Where(membership => membership.UserId == userId)
            .Join(context.Set<ApplicationRole>(), membership => membership.RoleId, role => role.Id, (membership, role) => new { role.Id, role.Name })
            .ToListAsync(cancellationToken);

        var roleIds = roles.Select(role => role.Id).ToList();
        var claims = await context.Set<IdentityRoleClaim<string>>()
            .Where(claim => roleIds.Contains(claim.RoleId) && claim.ClaimType == Permission.ClaimType)
            .Select(claim => new { claim.RoleId, claim.ClaimValue })
            .ToListAsync(cancellationToken);

        return [.. roles
            .OrderBy(role => role.Name, StringComparer.OrdinalIgnoreCase)
            .Select(role => new RolePermissions(
                role.Id,
                role.Name,
                claims
                    .Where(claim => claim.RoleId == role.Id && !string.IsNullOrEmpty(claim.ClaimValue))
                    .Select(claim => claim.ClaimValue)
                    .ToHashSet(StringComparer.Ordinal)))];
    }

    public async Task<IReadOnlySet<string>> GetRolePermissionsAsync(string roleId, CancellationToken cancellationToken = default)
    {
        await using var context = contextFactory.GetContext();
        var values = await context.Set<IdentityRoleClaim<string>>()
            .Where(claim => claim.RoleId == roleId && claim.ClaimType == Permission.ClaimType)
            .Select(claim => claim.ClaimValue)
            .ToListAsync(cancellationToken);

        return values.Where(value => !string.IsNullOrEmpty(value)).ToHashSet(StringComparer.Ordinal);
    }

    public async Task<Result> SetUserPermissionsAsync(string userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        await using var context = contextFactory.GetContext();
        if (!await context.Set<ApplicationUser>().AnyAsync(user => user.Id == userId, cancellationToken))
        {
            return Result.NotFound("That user no longer exists.");
        }

        var claims = context.Set<IdentityUserClaim<string>>();
        var current = await claims
            .Where(claim => claim.UserId == userId && claim.ClaimType == Permission.ClaimType)
            .ToListAsync(cancellationToken);

        var desired = Normalize(permissions);
        claims.RemoveRange(current.Where(claim => !desired.Contains(claim.ClaimValue)));
        claims.AddRange(desired
            .Where(value => !current.Any(claim => claim.ClaimValue == value))
            .Select(value => new IdentityUserClaim<string> { UserId = userId, ClaimType = Permission.ClaimType, ClaimValue = value }));

        await context.SaveChangesAsync(cancellationToken);
        Invalidate(userId);
        logger.LogInformation("Set the permissions of user {UserId} to {Permissions}.", userId, desired);
        return Result.Success();
    }

    public async Task<Result> SetRolePermissionsAsync(string roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        await using var context = contextFactory.GetContext();
        if (!await context.Set<ApplicationRole>().AnyAsync(role => role.Id == roleId, cancellationToken))
        {
            return Result.NotFound("That role no longer exists.");
        }

        var claims = context.Set<IdentityRoleClaim<string>>();
        var current = await claims
            .Where(claim => claim.RoleId == roleId && claim.ClaimType == Permission.ClaimType)
            .ToListAsync(cancellationToken);

        var desired = Normalize(permissions);
        claims.RemoveRange(current.Where(claim => !desired.Contains(claim.ClaimValue)));
        claims.AddRange(desired
            .Where(value => !current.Any(claim => claim.ClaimValue == value))
            .Select(value => new IdentityRoleClaim<string> { RoleId = roleId, ClaimType = Permission.ClaimType, ClaimValue = value }));

        await context.SaveChangesAsync(cancellationToken);
        InvalidateAll();
        logger.LogInformation("Set the permissions of role {RoleId} to {Permissions}.", roleId, desired);
        return Result.Success();
    }

    public bool TryGetCached(string userId, out GrantedPermissions granted)
    {
        if (string.IsNullOrEmpty(userId))
        {
            granted = GrantedPermissions.None;
            return false;
        }

        return cache.TryGetValue(CacheKey(userId), out granted);
    }

    public void Invalidate(string userId)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            cache.Remove(CacheKey(userId));
        }
    }

    public void InvalidateAll() => Interlocked.Increment(ref generation);

    private async Task<List<ApplicationRole>> AdministratorRolesAsync(IEnumerable<string> roleIds, CancellationToken cancellationToken)
    {
        var ids = roleIds.ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        await using var context = contextFactory.GetContext();
        return await context.Set<ApplicationRole>()
            .Where(role => ids.Contains(role.Id) && role.Name == InfernoSecurityConstants.Roles.Administrators)
            .ToListAsync(cancellationToken);
    }

    private static HashSet<string> Normalize(IEnumerable<string> permissions) =>
        (permissions ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToHashSet(StringComparer.Ordinal);

    private string CacheKey(string userId) => $"Inferno.Permissions:{Volatile.Read(ref generation)}:{userId}";
}
