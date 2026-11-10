using System.Security.Claims;

namespace Inferno.Web.Security.Permissions;

/// <summary>
/// Whether a menu or dashboard link should be shown. The store cache expires while a circuit is still open,
/// and the menu is drawn synchronously, so the last answer for this circuit is kept.
/// A link with no policies is shown. A link whose grants cannot be read is hidden.
/// </summary>
public sealed class PermissionVisibility(IPermissionStore store, IPermissionCatalog catalog)
{
    private readonly Dictionary<string, GrantedPermissions> remembered = new(StringComparer.Ordinal);

    public bool Allows(ClaimsPrincipal user, IEnumerable<string> policies)
    {
        string[] required = Required(policies);
        if (required.Length == 0)
        {
            return true;
        }

        return TryGet(user, out var granted) && required.All(policy => catalog.IsGranted(policy, granted));
    }

    /// <summary>Reads the grants into the circuit memory. Call this before drawing links that cannot wait.</summary>
    public async Task EnsureLoadedAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        string userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        remembered[userId] = await store.GetGrantedAsync(userId, cancellationToken);
    }

    private bool TryGet(ClaimsPrincipal user, out GrantedPermissions granted)
    {
        string userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            granted = GrantedPermissions.None;
            return false;
        }

        if (store.TryGetCached(userId, out granted))
        {
            remembered[userId] = granted;
            return true;
        }

        return remembered.TryGetValue(userId, out granted);
    }

    private static string[] Required(IEnumerable<string> policies) =>
        (policies ?? [])
            .Where(policy => !string.IsNullOrWhiteSpace(policy))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
}