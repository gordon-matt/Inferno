using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Inferno.Web.Security.Permissions;

/// <summary>Satisfied when the user holds <see cref="Permission"/>, directly, through a role, or through a permission that implies it.</summary>
/// <param name="Permission">The permission name the user must hold.</param>
public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;

/// <summary>Succeeds a <see cref="PermissionRequirement"/> when the user's grants cover it.</summary>
public sealed class PermissionAuthorizationHandler(IPermissionStore store, IPermissionCatalog catalog)
    : AuthorizationHandler<PermissionRequirement>
{
    /// <summary>
    /// Succeeds the requirement when the signed-in user's grants cover it.
    /// A user with no id is left undecided.
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <param name="requirement">The permission being checked.</param>
    /// <returns>A task that completes when the check has finished.</returns>
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

    /// <summary>
    /// Returns a registered policy, or builds one for a known permission name.
    /// A name that is neither a registered policy nor a permission returns <see langword="null"/>.
    /// </summary>
    /// <param name="policyName">The policy or permission name.</param>
    /// <returns>The policy, or <see langword="null"/>.</returns>
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
