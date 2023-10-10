using Inferno.Security;
using Inferno.Web.Security;
using InfernoCMS.Data;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace InfernoCMS.Identity.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfernoAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(StandardPolicies.AdminAccess, policy => policy.RequireClaim("Permission", "AdminAccess"));
                options.AddPolicy(StandardPolicies.FullAccess, policy => policy.RequireClaim("Permission", "FullAccess"));
                options.AddInfernoWebPolicies();
            });
        }

        public static IdentityBuilder AddInfernoIdentity(this IServiceCollection services)
        {
            //services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            return services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserStore<ApplicationUserStore>()
            .AddRoleStore<ApplicationRoleStore>()
            //.AddRoleValidator<ApplicationRoleValidator>()
            .AddDefaultTokenProviders();
        }

        public static AuthenticationBuilder AddInfernoJwtBearer(this AuthenticationBuilder builder, IConfiguration configuration)
        {
            return builder.AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
            });
        }
    }
}