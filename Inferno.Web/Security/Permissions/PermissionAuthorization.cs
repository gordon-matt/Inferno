using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Inferno.Web.Security.Permissions;

/// <summary>Satisfied when the user holds <see cref="Permission"/>, directly, through a role, or through a permission that implies it.</summary>
public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;

public sealed class PermissionAuthorizationHandler(IPermissionStore store, IPermissionCatalog catalog)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        string userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        var granted = await store.GetGrantedAsync(userId);
        if (catalog.IsGranted(requirement.Permission, granted))
        {
            context.Succeed(requirement);
        }
    }
}

/// <summary>
/// Turns any permission name into a policy on first use, so modules and plugins never register policies by hand.
/// Policies registered explicitly in <see cref="AuthorizationOptions"/> still take precedence.
/// </summary>
public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options, IPermissionCatalog catalog)
    : DefaultAuthorizationPolicyProvider(options)
{
    private readonly ConcurrentDictionary<string, AuthorizationPolicy> policies = new(StringComparer.Ordinal);

    public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);
        if (policy is not null || catalog.Find(policyName) is null)
        {
            return policy;
        }

        return policies.GetOrAdd(policyName, name => new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(name))
            .Build());
    }
}
