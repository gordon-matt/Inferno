using Inferno.Security;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Security.Permissions;

/// <summary>
/// Every permission the installed modules define, and the rules for what a set of grants covers.
/// When two providers use the same name, the first registration is kept.
/// </summary>
public interface IPermissionCatalog
{
    /// <summary>All permissions, ordered by category and then display name.</summary>
    IReadOnlyList<Permission> All { get; }

    /// <summary>The permission called <paramref name="name"/>, or <see langword="null"/> when no module defines it.</summary>
    /// <param name="name">The permission name. <see langword="null"/> returns <see langword="null"/>.</param>
    /// <returns>The permission, or <see langword="null"/>.</returns>
    Permission Find(string name);

    /// <summary>
    /// <paramref name="names"/> plus every permission they imply. A permission that grants admin access also adds
    /// <see cref="StandardPolicies.AdminAccess"/>. A name no module defines is kept and implies nothing further.
    /// </summary>
    /// <param name="names">Permission names to expand. <see langword="null"/> expands nothing.</param>
    /// <returns>The expanded names.</returns>
    IReadOnlySet<string> Expand(IEnumerable<string> names);

    /// <summary>
    /// Whether <paramref name="granted"/> covers <paramref name="permission"/>.
    /// A host administrator, or a set that expands to <see cref="StandardPolicies.FullAccess"/>, covers every permission.
    /// A tenant administrator covers every permission except <see cref="StandardPolicies.FullAccess"/>.
    /// <see langword="null"/> grants or an empty permission do not.
    /// </summary>
    /// <param name="permission">The permission name being checked.</param>
    /// <param name="granted">The user's grants.</param>
    /// <returns><see langword="true"/> when the grants cover <paramref name="permission"/>.</returns>
    bool IsGranted(string permission, GrantedPermissions granted);
}

/// <inheritdoc cref="IPermissionCatalog" />
public sealed class PermissionCatalog : IPermissionCatalog
{
    private readonly Dictionary<string, Permission> byName = new(StringComparer.Ordinal);

    /// <summary>Reads every provider once. A duplicate name is logged and the first definition is kept.</summary>
    /// <param name="providers">The registered permission providers.</param>
    /// <param name="logger">Receives a warning for each duplicate name.</param>
    public PermissionCatalog(IEnumerable<IPermissionProvider> providers, ILogger<PermissionCatalog> logger)
    {
        foreach (var provider in providers)
        {
            foreach (var permission in provider.GetPermissions())
            {
                if (!byName.TryAdd(permission.Name, permission))
                {
                    logger.LogWarning(
                        "{Provider} defines the permission {Permission}, which another module already defined. The first definition is kept.",
                        provider.GetType().Name,
                        permission.Name);
                }
            }
        }

        All = [.. byName.Values
            .OrderBy(permission => permission.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(permission => permission.DisplayName, StringComparer.OrdinalIgnoreCase)];
    }

    /// <inheritdoc />
    public IReadOnlyList<Permission> All { get; }

    /// <inheritdoc />
    public Permission Find(string name) =>
        name is not null && byName.TryGetValue(name, out var permission) ? permission : null;

    /// <inheritdoc />
    public IReadOnlySet<string> Expand(IEnumerable<string> names)
    {
        var expanded = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>(names ?? []);
        while (pending.Count > 0)
        {
            string name = pending.Pop();
            if (!expanded.Add(name) || Find(name) is not { } permission)
            {
                continue;
            }

            foreach (string implied in permission.Implies)
            {
                pending.Push(implied);
            }

            if (permission.GrantsAdminAccess)
            {
                pending.Push(StandardPolicies.AdminAccess);
            }
        }

        return expanded;
    }

    /// <inheritdoc />
    public bool IsGranted(string permission, GrantedPermissions granted)
    {
        if (granted is null || string.IsNullOrEmpty(permission))
        {
            return false;
        }

        if (granted.IsHostAdministrator)
        {
            return true;
        }

        var expanded = Expand(granted.Permissions);
        if (expanded.Contains(StandardPolicies.FullAccess))
        {
            return true;
        }

        // A tenant's administrators hold everything in their tenant, but not the host-wide FullAccess.
        return permission == StandardPolicies.FullAccess
            ? false
            : granted.IsAdministrator || expanded.Contains(permission);
    }
}
