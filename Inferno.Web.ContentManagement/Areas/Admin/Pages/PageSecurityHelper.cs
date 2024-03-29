﻿using System.Security.Principal;
using Dependo;
using Extenso.Collections;
using Inferno.Security.Membership;
using Inferno.Web.ContentManagement.Areas.Admin.Blog;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Newtonsoft.Json.Linq;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages
{
    public static class PageSecurityHelper
    {
        public static async Task<bool> CheckUserHasAccessToBlog(IPrincipal user)
        {
            var blogSettings = EngineContext.Current.Resolve<BlogSettings>();

            if (!blogSettings.RoleIds.IsNullOrEmpty())
            {
                var membershipService = EngineContext.Current.Resolve<IMembershipService>();
                var roles = await membershipService.GetRolesByIdsAsync(blogSettings.RoleIds);

                var roleNames = roles
                    .Select(x => x.Name)
                    .Where(x => x != null)
                    .ToList();

                bool authorized = roleNames.Any(user.IsInRole);
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
                    var membershipService = EngineContext.Current.Resolve<IMembershipService>();

                    var roleIds = selectedRoles.Split(',');
                    var roles = await membershipService.GetRolesByIdsAsync(roleIds);

                    var infernoUser = await membershipService.GetUserByNameAsync(page.TenantId, user.Identity.Name);
                    var userRoles = await membershipService.GetRolesForUserAsync(infernoUser.Id);
                    var userRoleIds = userRoles.Select(x => x.Id);

                    return userRoleIds.ContainsAny(selectedRoles.Split(','));
                }
            }
            return true;
        }
    }
}