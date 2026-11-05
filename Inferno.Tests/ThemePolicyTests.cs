using Inferno.Security;
using Inferno.Web.Security;
using Inferno.Web.Security.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Xunit;

namespace Inferno.Tests;

public class ThemePolicyTests
{
    private static AuthorizationPolicy PolicyFor(string name)
    {
        var options = new AuthorizationOptions();
        options.AddInfernoWebPolicies();

        return options.GetPolicy(name);
    }

    private static string RequiredPermission(string policyName) => PolicyFor(policyName)
        .Requirements
        .OfType<PermissionRequirement>()
        .Single()
        .Permission;

    [Theory]
    [InlineData(InfernoWebAuthorizationOptionsExtensions.ThemeView, InfernoWebPolicies.ThemesRead)]
    [InlineData(InfernoWebAuthorizationOptionsExtensions.ThemeManage, InfernoWebPolicies.ThemesWrite)]
    [InlineData(InfernoWebAuthorizationOptionsExtensions.ThemeManageCatalog, StandardPolicies.FullAccess)]
    public void AddInfernoWebPolicies_ThemePolicy_RequiresMatchingPermission(string policyName, string permission)
    {
        Assert.Equal(permission, RequiredPermission(policyName));
    }

    [Theory]
    [InlineData(InfernoWebAuthorizationOptionsExtensions.ThemeView)]
    [InlineData(InfernoWebAuthorizationOptionsExtensions.ThemeManage)]
    [InlineData(InfernoWebAuthorizationOptionsExtensions.ThemeManageCatalog)]
    public void AddInfernoWebPolicies_ThemePolicy_RequiresSignedInUser(string policyName)
    {
        Assert.Contains(PolicyFor(policyName).Requirements, requirement => requirement is DenyAnonymousAuthorizationRequirement);
    }

    [Fact]
    public void AddInfernoWebPolicies_CatalogPolicy_IsNotSatisfiedByThemeWritePermission()
    {
        Assert.NotEqual(RequiredPermission(InfernoWebAuthorizationOptionsExtensions.ThemeManage), RequiredPermission(InfernoWebAuthorizationOptionsExtensions.ThemeManageCatalog));
    }
}
