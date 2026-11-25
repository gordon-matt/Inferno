using Inferno.Web.Security.Permissions;
using Kourier.Core;
using Microsoft.AspNetCore.Authorization;

namespace InfernoCMS.Infrastructure.Kourier;

/// <summary>
/// Registers each Kourier policy name so Inferno's permission system answers it, and so Kourier does not
/// fall back to role names from configuration.
/// </summary>
public static class KourierAuthorizationOptionsExtensions
{
    /// <summary>Adds a policy for every <see cref="KourierPolicies"/> name, each requiring the matching permission.</summary>
    /// <param name="options">The authorization options.</param>
    public static void AddInfernoKourierPolicies(this AuthorizationOptions options)
    {
        foreach (string policy in KourierPolicies.All)
        {
            options.AddPolicy(policy, builder => builder
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(policy)));
        }
    }
}