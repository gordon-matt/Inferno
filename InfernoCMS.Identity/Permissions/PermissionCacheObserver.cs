using IdentityNexus.Services.Extensibility;
using Inferno.Web.Security.Permissions;

namespace InfernoCMS.Identity.Permissions;

/// <summary>
/// Clears cached grants when IdentityNexus changes a user or role, because role membership, role names and
/// deletions all change what a user holds.
/// </summary>
/// <param name="store">The grant cache to clear.</param>
public sealed class PermissionCacheObserver(IPermissionStore store) : IIdentityChangeObserver
{
    /// <summary>Drops the cached grants for a changed user, or every cached grant when a role changes.</summary>
    /// <param name="change">The user or role IdentityNexus just saved.</param>
    /// <param name="cancellationToken">Not observed.</param>
    /// <returns>A completed task.</returns>
    public Task OnChangedAsync(IdentityChange change, CancellationToken cancellationToken = default)
    {
        if (change.Kind == IdentitySubjectKind.User)
        {
            store.Invalidate(change.Id);
        }
        else
        {
            store.InvalidateAll();
        }

        return Task.CompletedTask;
    }
}
