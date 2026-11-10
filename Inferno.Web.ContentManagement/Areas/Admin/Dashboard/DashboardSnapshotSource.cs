using System.Globalization;
using BlogKrafter;
using BlogKrafter.Data.Entities;
using Extenso.Data.Entity;
using Inferno.Security.Membership;
using Inferno.Web.Configuration;
using Inferno.Web.Security;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.ContentManagement.Areas.Admin.Dashboard;

/// <summary>Loads the current tenant once per circuit refresh and shares it with every widget.</summary>
public sealed class DashboardSnapshotSource(
    IWorkContext workContext,
    IAuthorizationService authorization,
    AuthenticationStateProvider authentication,
    SiteSettings siteSettings,
    IRepository<Page> pages,
    IRepository<PageVersion> versions,
    IMembershipService membership,
    IServiceProvider services,
    ILogger<DashboardSnapshotSource> logger)
{
    private Task<DashboardSnapshot> current;

    public Task<DashboardSnapshot> GetAsync(bool refresh)
    {
        if (refresh)
        {
            current = null;
        }

        return current ??= LoadAsync();
    }

    private async Task<DashboardSnapshot> LoadAsync()
    {
        try
        {
            int tenantId = workContext.CurrentTenant.Id;
            var user = (await authentication.GetAuthenticationStateAsync()).User;
            bool canReadPages = (await authorization.AuthorizeAsync(user, CmsConstants.Policies.PagesRead)).Succeeded;
            bool canReadPosts = (await authorization.AuthorizeAsync(user, CmsConstants.Policies.BlogRead)).Succeeded;
            bool canReadUsers = (await authorization.AuthorizeAsync(user, InfernoWebPolicies.MembershipManage)).Succeeded;

            var input = new DashboardSnapshotInput
            {
                CanReadPages = canReadPages,
                CanReadPosts = canReadPosts,
                CanReadUsers = canReadUsers,
                BlogEnabled = siteSettings.EnableBlog,
                UtcNow = DateTime.UtcNow,
                Pages = canReadPages ? LoadPages(tenantId) : [],
                Versions = canReadPages ? LoadVersions(tenantId) : [],
                Posts = canReadPosts && siteSettings.EnableBlog ? LoadPosts(tenantId) : [],
                Users = canReadUsers ? await LoadUsersAsync(tenantId) : [],
                Roles = canReadUsers ? await LoadRolesAsync(tenantId) : [],
            };

            return DashboardSnapshotBuilder.Build(input);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "The dashboard for tenant {TenantId} could not be loaded.", workContext.CurrentTenant?.Id);
            return DashboardSnapshot.Failed("The dashboard could not be loaded.");
        }
    }

    private IReadOnlyList<DashboardPageRow> LoadPages(int tenantId)
    {
        using var connection = pages.OpenConnection();
        return connection
            .Query(page => page.TenantId == tenantId)
            .Select(page => new DashboardPageRow(page.Id, page.Name, page.IsEnabled))
            .ToList();
    }

    private IReadOnlyList<DashboardVersionRow> LoadVersions(int tenantId)
    {
        using var connection = versions.OpenConnection();
        return connection
            .Query(version => version.TenantId == tenantId)
            .Select(version => new DashboardVersionRow(
                version.Id,
                version.PageId,
                version.Status,
                version.Title,
                version.Fields,
                version.DateModifiedUtc))
            .ToList();
    }

    private IReadOnlyList<DashboardPostRow> LoadPosts(int tenantId)
    {
        var posts = services.GetService<IRepository<BlogPost>>();
        if (posts is null)
        {
            return [];
        }

        try
        {
            string tenantKey = tenantId.ToString(CultureInfo.InvariantCulture);
            using var connection = posts.OpenConnection();
            return connection
                .Query(post => post.TenantId == tenantKey)
                .Select(post => new DashboardPostRow(
                    post.Id,
                    post.Title,
                    post.Status == PostStatus.Draft,
                    post.PublishedAt,
                    post.UpdatedAt,
                    post.AuthorName,
                    post.MetaTitle,
                    post.MetaDescription,
                    post.CoverImageUrl,
                    post.CoverImageAlt))
                .ToList();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Dashboard could not read blog posts for tenant {TenantId}.", tenantId);
            return [];
        }
    }

    private async Task<IReadOnlyList<DashboardUserRow>> LoadUsersAsync(int tenantId)
    {
        var users = await membership.GetAllUsersAsync(tenantId);
        return users
            .Select(user => new DashboardUserRow(user.UserName, user.Email, user.IsLockedOut))
            .ToList();
    }

    private async Task<IReadOnlyList<DashboardRoleRow>> LoadRolesAsync(int tenantId)
    {
        var roles = await membership.GetAllRolesAsync(tenantId);
        var counts = new List<DashboardRoleRow>();
        foreach (var role in roles)
        {
            var members = await membership.GetUsersByRoleIdAsync(role.Id);
            counts.Add(new DashboardRoleRow(role.Name, members.Count()));
        }

        return counts;
    }
}
