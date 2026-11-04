using Inferno.Security;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Security.Permissions;

/// <summary>Every permission the installed modules define, and the rules for what a set of grants covers.</summary>
public interface IPermissionCatalog
{
    /// <summary>All permissions, ordered by category and then display name.</summary>
    IReadOnlyList<Permission> All { get; }

    /// <summary>The permission called <paramref name="name"/>, or null when no module defines it.</summary>
    Permission Find(string name);

    /// <summary><paramref name="names"/> plus every permission they imply, directly or through another.</summary>
    IReadOnlySet<string> Expand(IEnumerable<string> names);

    /// <summary>Whether <paramref name="granted"/> covers <paramref name="permission"/>.</summary>
    bool IsGranted(string permission, GrantedPermissions granted);
}

public sealed class PermissionCatalog : IPermissionCatalog
{
    private readonly Dictionary<string, Permission> byName = new(StringComparer.Ordinal);

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

    public IReadOnlyList<Permission> All { get; }

    public Permission Find(string name) =>
        name is not null && byName.TryGetValue(name, out var permission) ? permission : null;

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
