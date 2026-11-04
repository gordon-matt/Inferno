using IdentityNexus.Services.Extensibility;
using Inferno.Web.Security.Permissions;

namespace InfernoCMS.Identity.Permissions;

/// <summary>
/// Clears cached grants when IdentityNexus changes a user or role, because role membership, role names and
/// deletions all change what a user holds.
/// </summary>
public sealed class PermissionCacheObserver(IPermissionStore store) : IIdentityChangeObserver
{
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
