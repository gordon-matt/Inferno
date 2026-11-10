using System.Security.Claims;
using Ardalis.Result;
using Heroicsoft.NetForge.Abstractions.Navigation;
using Inferno.Security;
using Inferno.Web.Areas.Admin;
using Inferno.Web.ContentManagement;
using Inferno.Web.ContentManagement.Security;
using Inferno.Web.Navigation;
using Inferno.Web.Security;
using Inferno.Web.Security.Permissions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Inferno.Tests;

public class AdminLinkAccessTests
{
    private static readonly PermissionCatalog Catalog = new(
        [new InfernoWebPermissionProvider(), new CmsPermissionProvider()],
        NullLogger<PermissionCatalog>.Instance);

    [Fact]
    public void Allows_NoPolicies_IsVisibleWithoutGrants()
    {
        var visibility = Visibility(cached: false);

        Assert.True(visibility.Allows(User("editor"), []));
    }

    [Fact]
    public void Allows_GrantsNotLoaded_HidesTheLink()
    {
        var visibility = Visibility(cached: false);

        Assert.False(visibility.Allows(User("editor"), [InfernoWebPolicies.MembershipManage]));
    }

    [Fact]
    public void Allows_HeldPermission_IsVisible()
    {
        var visibility = Visibility(cached: true, InfernoWebPolicies.MembershipManage);

        Assert.True(visibility.Allows(User("editor"), [InfernoWebPolicies.MembershipManage]));
    }

    [Fact]
    public void Allows_MissingPermission_IsHidden()
    {
        var visibility = Visibility(cached: true, CmsConstants.Policies.PagesRead);

        Assert.False(visibility.Allows(User("editor"), [InfernoWebPolicies.MembershipManage]));
        Assert.False(visibility.Allows(User("editor"), [StandardPolicies.FullAccess]));
    }

    [Fact]
    public void Allows_ImpliedPermission_IsVisible()
    {
        var visibility = Visibility(cached: true, InfernoWebPolicies.SettingsWrite);

        Assert.True(visibility.Allows(User("editor"), [InfernoWebPolicies.SettingsRead]));
    }

    [Fact]
    public void Allows_CacheExpired_KeepsTheLastAnswer()
    {
        var store = new GrantStore { Granted = Grants(InfernoWebPolicies.MembershipManage), Cached = true };
        var visibility = new PermissionVisibility(store, Catalog);
        ClaimsPrincipal user = User("editor");

        Assert.True(visibility.Allows(user, [InfernoWebPolicies.MembershipManage]));

        store.Cached = false;

        Assert.True(visibility.Allows(user, [InfernoWebPolicies.MembershipManage]));
    }

    [Fact]
    public async Task VisibleHrefs_LimitedUser_HidesMembershipTenantsAndBlog()
    {
        IReadOnlySet<string> hrefs = await Hrefs(Grants(CmsConstants.Policies.PagesRead));

        Assert.Contains("/admin/pages/index", hrefs);
        Assert.DoesNotContain("/admin/membership", hrefs);
        Assert.DoesNotContain("/admin/tenants/index", hrefs);
        Assert.DoesNotContain("/admin/blog/posts", hrefs);
    }

    [Fact]
    public async Task VisibleHrefs_TenantAdministrator_SeesMembershipButNotTenants()
    {
        IReadOnlySet<string> hrefs = await Hrefs(new GrantedPermissions(IsAdministrator: true, IsHostAdministrator: false, new HashSet<string>()));

        Assert.Contains("/admin/membership", hrefs);
        Assert.Contains("/admin/blog/posts", hrefs);
        Assert.DoesNotContain("/admin/tenants/index", hrefs);
    }

    [Fact]
    public async Task VisibleHrefs_HostAdministrator_SeesTenants()
    {
        IReadOnlySet<string> hrefs = await Hrefs(new GrantedPermissions(IsAdministrator: true, IsHostAdministrator: true, new HashSet<string>()));

        Assert.Contains("/admin/tenants/index", hrefs);
        Assert.Contains("/admin/membership", hrefs);
    }

    [Fact]
    public void Key_TrimsTrailingSlashAndQuery()
    {
        Assert.Equal("/admin/pages/index", AdminLinkAccess.Key("/admin/pages/index/?tab=1"));
    }

    [Fact]
    public void Get_MembershipItem_UsesItsOwnPolicy()
    {
        var store = new GrantStore { Granted = Grants(CmsConstants.Policies.PagesRead), Cached = true };
        var visibility = new PermissionVisibility(store, Catalog);
        var catalog = new AdminMenuCatalog(new FakeNavigation(SampleMenu()), visibility);

        ForgeNavItem membership = catalog.Get(AdminMenuCatalog.MenuName).Sections
            .SelectMany(section => section.Items)
            .Single(item => item.Title == "Membership");

        Assert.False(membership.IsVisible(User("editor")));
    }

    private static Task<IReadOnlySet<string>> Hrefs(GrantedPermissions granted)
    {
        var store = new GrantStore { Granted = granted };
        var access = new AdminLinkAccess(new FakeNavigation(SampleMenu()), new PermissionVisibility(store, Catalog));
        return access.VisibleHrefsAsync(User("editor"));
    }

    private static PermissionVisibility Visibility(bool cached, params string[] permissions)
    {
        var store = new GrantStore { Granted = Grants(permissions), Cached = cached };
        return new PermissionVisibility(store, Catalog);
    }

    private static GrantedPermissions Grants(params string[] permissions) =>
        new(false, false, permissions.ToHashSet(StringComparer.Ordinal));

    private static ClaimsPrincipal User(string id) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id)], authenticationType: "test"));

    private static MenuItem[] SampleMenu() =>
    [
        new()
        {
            Text = "Membership",
            Url = "/admin/membership",
            Policies = [InfernoWebPolicies.MembershipManage],
            Items = [],
        },
        new()
        {
            Text = "CMS",
            Items =
            [
                new MenuItem
                {
                    Text = "Blog",
                    Policies = [CmsConstants.Policies.BlogWrite],
                    Items =
                    [
                        new MenuItem
                        {
                            Text = "Posts",
                            Url = "/admin/blog/posts",
                            Policies = [CmsConstants.Policies.BlogWrite],
                            Items = [],
                        },
                    ],
                },
                new MenuItem
                {
                    Text = "Pages",
                    Url = "/admin/pages/index",
                    Policies = [CmsConstants.Policies.PagesRead],
                    Items = [],
                },
            ],
        },
        new()
        {
            Text = "Tenants",
            Url = "/admin/tenants/index",
            Policies = [StandardPolicies.FullAccess],
            Items = [],
        },
    ];

    private sealed class FakeNavigation(params MenuItem[] items) : INavigationManager
    {
        public IEnumerable<MenuItem> BuildMenu(string menuName) => items;

        public string GetUrl(string menuItemUrl) => menuItemUrl;
    }

    private sealed class GrantStore : IPermissionStore
    {
        public GrantedPermissions Granted { get; set; } = GrantedPermissions.None;

        public bool Cached { get; set; }

        public Task<GrantedPermissions> GetGrantedAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(string.IsNullOrEmpty(userId) ? GrantedPermissions.None : Granted);

        public bool TryGetCached(string userId, out GrantedPermissions granted)
        {
            granted = Granted;
            return Cached && !string.IsNullOrEmpty(userId);
        }

        public Task<IReadOnlySet<string>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<RolePermissions>> GetUserRolePermissionsAsync(string userId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlySet<string>> GetRolePermissionsAsync(string roleId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Result> SetUserPermissionsAsync(string userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Result> SetRolePermissionsAsync(string roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public void Invalidate(string userId)
        {
        }

        public void InvalidateAll()
        {
        }
    }
}
