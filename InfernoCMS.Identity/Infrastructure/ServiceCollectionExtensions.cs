using Inferno.Security;
using Inferno.Web.Security;
using InfernoCMS.Data;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace InfernoCMS.Identity.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        // Any JWT key shorter than this (in bytes) is rejected by modern validators when
        // using HS256, and is cryptographically weak in any case.
        private const int MinimumJwtKeyLengthBytes = 32;

        // The placeholder key shipped in appsettings.json — we warn loudly if it's still
        // in use so nobody accidentally ships to production with it.
        private const string KnownInsecureJwtKey = "the quick brown fox jumped over the lazy dog";

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
            return services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
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

        public static AuthenticationBuilder AddInfernoJwtBearer(this AuthenticationBuilder builder, IConfiguration configuration)
        {
            ValidateJwtConfiguration(configuration, builder.Services);

            string jwtKey = configuration["Jwt:Key"];
            string jwtIssuer = configuration["Jwt:Issuer"];
            string jwtAudience = configuration["Jwt:Audience"] ?? jwtIssuer;

            return builder.AddJwtBearer(options =>
            {
                // In production this forces the token endpoint metadata to be fetched over
                // HTTPS. Automatically relaxed below for the Development environment.
                options.RequireHttpsMetadata = true;
                options.SaveToken = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? string.Empty)),
                    // The default 5-minute ClockSkew is way too generous for short-lived tokens.
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = System.Security.Claims.ClaimTypes.Name,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // Surface expiry to clients so they can attempt a refresh.
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers["Token-Expired"] = "true";
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }

        private static void ValidateJwtConfiguration(IConfiguration configuration, IServiceCollection services)
        {
            string jwtKey = configuration["Jwt:Key"];
            string jwtIssuer = configuration["Jwt:Issuer"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException(
                    "Jwt:Key is not configured. Set it via user secrets, environment variables, or appsettings.");
            }
            if (string.IsNullOrWhiteSpace(jwtIssuer))
            {
                throw new InvalidOperationException(
                    "Jwt:Issuer is not configured. Set it via user secrets, environment variables, or appsettings.");
            }

            // Log (on the first built ServiceProvider) — but we don't yet have a logger, so
            // defer this to startup using a hosted service.
            services.AddSingleton<IStartupFilter>(new JwtStartupValidator(jwtKey));
        }

        /// <summary>
        /// Emits a warning at application startup if a weak or default JWT key is in use.
        /// </summary>
        private sealed class JwtStartupValidator : IStartupFilter
        {
            private readonly string jwtKey;

            public JwtStartupValidator(string jwtKey)
            {
                this.jwtKey = jwtKey;
            }

            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return app =>
                {
                    var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
                    var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
                    var logger = loggerFactory.CreateLogger("Inferno.Security.Jwt");

                    int keyLengthBytes = Encoding.UTF8.GetByteCount(jwtKey);

                    if (string.Equals(jwtKey, KnownInsecureJwtKey, StringComparison.Ordinal))
                    {
                        string message =
                            "Jwt:Key is still set to the example/default value. " +
                            "Replace it immediately with a cryptographically strong random value " +
                            "(at least 32 bytes) stored in user secrets or environment variables.";

                        if (env.IsProduction())
                        {
                            logger.LogCritical(message);
                            throw new InvalidOperationException(message);
                        }

                        logger.LogWarning(message);
                    }
                    else if (keyLengthBytes < MinimumJwtKeyLengthBytes)
                    {
                        string message =
                            $"Jwt:Key is only {keyLengthBytes} bytes long. " +
                            $"HS256 requires at least {MinimumJwtKeyLengthBytes} bytes for security.";

                        if (env.IsProduction())
                        {
                            logger.LogCritical(message);
                            throw new InvalidOperationException(message);
                        }

                        logger.LogWarning(message);
                    }

                    next(app);
                };
            }
        }
    }
}
