using System.Security.Principal;
using Dependo;
using Extenso.Collections;
using Inferno.Security.Membership;
using Inferno.Web.ContentManagement.Areas.Admin.Blog;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Newtonsoft.Json.Linq;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages;

public static class PageSecurityHelper
{
    /// <summary>The roles a page is limited to, from <see cref="Page.AccessRestrictions"/>. Empty means everyone.</summary>
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
    public static string CreateAccessRestrictions(IEnumerable<string> roleIds)
    {
        var ids = roleIds.Where(id => !string.IsNullOrEmpty(id)).ToList();
        return ids.Count == 0
            ? null
            : new JObject { ["Roles"] = string.Join(',', ids) }.ToString(Newtonsoft.Json.Formatting.None);
    }

    public static async Task<bool> CheckUserHasAccessToBlog(IPrincipal user)
    {
        var blogSettings = DependoResolver.Instance.Resolve<BlogSettings>();

        if (!blogSettings.RoleIds.IsNullOrEmpty())
        {
            var membershipService = DependoResolver.Instance.Resolve<IMembershipService>();
            var roles = await membershipService.GetRolesByIdsAsync(blogSettings.RoleIds);

            var roleNames = roles
                .Select(x => x.Name)
                .Where(x => x != null)
                .ToList();

            bool authorized = user?.Identity?.IsAuthenticated == true && roleNames.Any(user.IsInRole);
            if (!authorized)
            {
                return false;
            }
        }
        return true;
    }

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