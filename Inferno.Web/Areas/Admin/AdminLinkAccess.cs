using System.Security.Claims;
using Inferno.Web.Navigation;
using Inferno.Web.Security.Permissions;

namespace Inferno.Web.Areas.Admin;

/// <summary>
/// The admin addresses the current user may open. Dashboard tiles use the same policies as the menu,
/// so a tile disappears together with the link that points at it.
/// </summary>
public sealed class AdminLinkAccess(INavigationManager navigation, PermissionVisibility visibility)
{
    public async Task<IReadOnlySet<string>> VisibleHrefsAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        await visibility.EnsureLoadedAsync(user, cancellationToken);

        var hrefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Collect(navigation.BuildMenu(InfernoWebConstants.Areas.Admin), user, hrefs);
        return hrefs;
    }

    public static string Key(string href)
    {
        if (string.IsNullOrWhiteSpace(href))
        {
            return string.Empty;
        }

        string value = href.Trim();
        int hash = value.IndexOf('#');
        if (hash >= 0)
        {
            value = value[..hash];
        }

        int query = value.IndexOf('?');
        if (query >= 0)
        {
            value = value[..query];
        }

        if (value.Length > 1)
        {
            value = value.TrimEnd('/');
        }

        return value;
    }

    private void Collect(IEnumerable<MenuItem> items, ClaimsPrincipal user, HashSet<string> hrefs)
    {
        if (items is null)
        {
            return;
        }

        foreach (MenuItem item in items)
        {
            // A denied parent hides its children, matching the menu.
            if (!visibility.Allows(user, item.Policies))
            {
                continue;
            }

            string href = string.IsNullOrWhiteSpace(item.Href) ? navigation.GetUrl(item.Url) : item.Href;
            string key = Key(href);
            if (key.Length > 0)
            {
                hrefs.Add(key);
            }

            Collect(item.Items, user, hrefs);
        }
    }
}
