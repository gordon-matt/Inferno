using IdentityNexus.Services;
using Inferno.Web.Security.Permissions;
using InfernoCMS.Data;
using InfernoCMS.Data.Entities;
using InfernoCMS.Identity.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace InfernoCMS.Identity.Infrastructure;

/// <summary>Registers Inferno Identity and permission authorization.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Authorizes with permissions: every permission a module declares becomes a policy, and grants are read
    /// from the Identity claim tables. Administrators hold every permission without being granted any.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services"/>, so registration can be chained.</returns>
    public static IServiceCollection AddInfernoAuthorization(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddInfernoPermissions();
        services.AddSingleton<IPermissionStore, PermissionStore>();
        services.AddIdentityNexusObserver<PermissionCacheObserver>();
        return services;
    }

    /// <summary>
    /// Registers application users and roles with confirmed accounts, a password and lockout policy, and the Inferno stores.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The Identity builder, so token providers and UI can be added.</returns>
    public static IdentityBuilder AddInfernoIdentity(this IServiceCollection services) =>
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;

            // Password policy — reasonable defaults. Can be overridden by the host app.
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;

            // Lockout policy — protect against brute-force password attacks.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddUserStore<ApplicationUserStore>()
        .AddRoleStore<ApplicationRoleStore>()
        //.AddRoleValidator<ApplicationRoleValidator>()
        .AddDefaultTokenProviders();
}
