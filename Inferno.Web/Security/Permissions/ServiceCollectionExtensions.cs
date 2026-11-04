using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Inferno.Web.Security.Permissions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Makes every permission from a registered <see cref="IPermissionProvider"/> usable as a policy.
    /// The host still registers an <see cref="IPermissionStore"/>, which knows its Identity tables.
    /// </summary>
    public static IServiceCollection AddInfernoPermissions(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.TryAddSingleton<IPermissionCatalog, PermissionCatalog>();
        services.Replace(ServiceDescriptor.Singleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAuthorizationHandler, PermissionAuthorizationHandler>());
        return services;
    }
}
