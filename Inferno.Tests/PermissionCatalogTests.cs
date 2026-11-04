using Inferno.Security;
using Inferno.Web.Security.Permissions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Inferno.Tests;

public class PermissionCatalogTests
{
    private const string READ = "Test.Things.Read";
    private const string WRITE = "Test.Things.Write";
    private const string PUBLIC_VIEW = "Test.Public.View";

    private static readonly PermissionCatalog Catalog = new(
        [new TestPermissionProvider()],
        NullLogger<PermissionCatalog>.Instance);

    [Fact]
    public void Expand_PermissionWithImplies_IncludesImpliedAndAdminAccess()
    {
        var expanded = Catalog.Expand([WRITE]);

        Assert.Contains(READ, expanded);
        Assert.Contains(StandardPolicies.AdminAccess, expanded);
    }

    [Fact]
    public void Expand_PermissionNotGrantingAdminAccess_ExcludesAdminAccess()
    {
        var expanded = Catalog.Expand([PUBLIC_VIEW]);

        Assert.DoesNotContain(StandardPolicies.AdminAccess, expanded);
    }

    [Fact]
    public void Expand_UnknownPermission_KeepsItWithoutImplications()
    {
        var expanded = Catalog.Expand(["Gone.Plugin.Permission"]);

        Assert.Equal(["Gone.Plugin.Permission"], expanded);
    }

    [Fact]
    public void IsGranted_ImpliedPermission_ReturnsTrue()
    {
        Assert.True(Catalog.IsGranted(READ, Grants(WRITE)));
    }

    [Fact]
    public void IsGranted_ImplyingPermissionNotHeld_ReturnsFalse()
    {
        Assert.False(Catalog.IsGranted(WRITE, Grants(READ)));
    }

    [Fact]
    public void IsGranted_NoGrants_ReturnsFalse()
    {
        Assert.False(Catalog.IsGranted(READ, GrantedPermissions.None));
    }

    [Fact]
    public void IsGranted_TenantAdministrator_GetsEverythingButFullAccess()
    {
        var granted = new GrantedPermissions(true, false, new HashSet<string>());

        Assert.True(Catalog.IsGranted(WRITE, granted));
        Assert.True(Catalog.IsGranted(StandardPolicies.AdminAccess, granted));
        Assert.False(Catalog.IsGranted(StandardPolicies.FullAccess, granted));
    }

    [Fact]
    public void IsGranted_HostAdministrator_GetsFullAccess()
    {
        var granted = new GrantedPermissions(true, true, new HashSet<string>());

        Assert.True(Catalog.IsGranted(StandardPolicies.FullAccess, granted));
    }

    [Fact]
    public void IsGranted_FullAccessClaim_GetsEverything()
    {
        var granted = Grants(StandardPolicies.FullAccess);

        Assert.True(Catalog.IsGranted(WRITE, granted));
        Assert.True(Catalog.IsGranted(StandardPolicies.FullAccess, granted));
    }

    [Fact]
    public void Constructor_DuplicateName_KeepsFirstDefinition()
    {
        var catalog = new PermissionCatalog(
            [new TestPermissionProvider(), new TestPermissionProvider("Other")],
            NullLogger<PermissionCatalog>.Instance);

        Assert.Equal("Things", catalog.Find(READ).Category);
        Assert.Equal(catalog.All.Count, catalog.All.Select(permission => permission.Name).Distinct().Count());
    }

    private static GrantedPermissions Grants(params string[] permissions) =>
        new(false, false, new HashSet<string>(permissions));

    private sealed class TestPermissionProvider(string category = "Things") : IPermissionProvider
    {
        public IEnumerable<Permission> GetPermissions() =>
        [
            new(StandardPolicies.AdminAccess, "General", "Admin access"),
            new(StandardPolicies.FullAccess, "General", "Full access"),
            new(READ, category, "Read things"),
            new(WRITE, category, "Write things") { Implies = [READ] },
            new(PUBLIC_VIEW, category, "View public things") { GrantsAdminAccess = false },
        ];
    }
}
