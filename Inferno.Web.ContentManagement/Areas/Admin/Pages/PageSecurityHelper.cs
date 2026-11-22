using System.Security.Principal;
using Dependo;
using Extenso.Collections;
using Inferno.Security.Membership;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Newtonsoft.Json.Linq;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages;

/// <summary>Reads and writes the role list stored on a page, and checks whether a visitor may view it.</summary>
public static class PageSecurityHelper
{
    /// <summary>The roles a page is limited to, from <see cref="Page.AccessRestrictions"/>. Empty means everyone.</summary>
    /// <param name="accessRestrictions">The stored JSON. Whitespace, or JSON that cannot be read, returns an empty list.</param>
    /// <returns>The role ids. Empty when the page is open to everyone.</returns>
    public static IReadOnlyList<string> GetRoleIds(string accessRestrictions)
    {
        if (string.IsNullOrWhiteSpace(accessRestrictions))
        {
            return [];
        }

        try
        {
            string roles = (string)JObject.Parse(accessRestrictions)["Roles"];
            return string.IsNullOrEmpty(roles) ? [] : roles.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
        catch (Newtonsoft.Json.JsonException)
        {
            return [];
        }
    }

    /// <summary>The value to store in <see cref="Page.AccessRestrictions"/>. Null when the page is open to everyone.</summary>
    /// <param name="roleIds">The role ids to store. Empty ids are omitted.</param>
    /// <returns>JSON with a Roles property, or <see langword="null"/> when no ids remain.</returns>
    public static string CreateAccessRestrictions(IEnumerable<string> roleIds)
    {
        var ids = roleIds.Where(id => !string.IsNullOrEmpty(id)).ToList();
        return ids.Count == 0
            ? null
            : new JObject { ["Roles"] = string.Join(',', ids) }.ToString(Newtonsoft.Json.Formatting.None);
    }

    /// <summary>Whether <paramref name="user"/> may view <paramref name="page"/>.</summary>
    /// <param name="page">The page. A disabled page is refused.</param>
    /// <param name="user">The visitor. An anonymous visitor is refused when the page lists roles.</param>
    /// <returns>
    /// <see langword="false"/> when the page is disabled, or when it lists roles and the user has none of them.
    /// <see langword="true"/> when the page lists no roles, or the user has one of them.
    /// </returns>
    public static async Task<bool> CheckUserHasAccessToPage(Page page, IPrincipal user)
    {
        if (!page.IsEnabled)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(page.AccessRestrictions))
        {
            dynamic accessRestrictions = JObject.Parse(page.AccessRestrictions);
            string selectedRoles = accessRestrictions.Roles;

            if (!string.IsNullOrEmpty(selectedRoles))
            {
                if (user?.Identity?.IsAuthenticated != true || string.IsNullOrEmpty(user.Identity.Name))
                {
                    return false;
                }

                var membershipService = DependoResolver.Instance.Resolve<IMembershipService>();

                var infernoUser = await membershipService.GetUserByNameAsync(page.TenantId, user.Identity.Name);
                if (infernoUser == null)
                {
                    return false;
                }

                var userRoles = await membershipService.GetRolesForUserAsync(infernoUser.Id);
                var userRoleIds = userRoles.Select(x => x.Id);

                return userRoleIds.ContainsAny(selectedRoles.Split(','));
            }
        }
        return true;
    }
}