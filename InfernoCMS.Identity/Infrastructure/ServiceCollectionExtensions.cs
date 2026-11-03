using Inferno.Security;
using Inferno.Web.Security;
using InfernoCMS.Data;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InfernoCMS.Identity.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void AddInfernoAuthorization(this IServiceCollection services, IConfiguration configuration) =>
        services.AddAuthorization(options =>
        {
            // Baseline policies. Anyone in the built-in Administrators role satisfies these automatically,
            // which lines up with how the seed "super admin" user gets provisioned in StartupTask.
            options.AddPolicy(StandardPolicies.AdminAccess, policy => policy.RequireAssertion(ctx =>
                ctx.User.IsInRole(InfernoSecurityConstants.Roles.Administrators) ||
                ctx.User.HasClaim(AuthorizationOptionsExtensions.PermissionClaimType, StandardPolicies.AdminAccess) ||
                ctx.User.HasClaim(AuthorizationOptionsExtensions.PermissionClaimType, StandardPolicies.FullAccess)));

            options.AddPolicy(StandardPolicies.FullAccess, policy => policy.RequireAssertion(ctx =>
                ctx.User.IsInRole(InfernoSecurityConstants.Roles.Administrators) ||
                ctx.User.HasClaim(AuthorizationOptionsExtensions.PermissionClaimType, StandardPolicies.FullAccess)));

            options.AddInfernoWebPolicies();
        });

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
