using Inferno.Web.Identity;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InfernoCMS.Identity.Services
{
    public class TokenService : ITokenService
    {
        // Tokens issued by this service are valid for this duration. Keep in sync with
        // any client-side caching (e.g. RadzenODataService).
        public static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(120);

        private readonly IConfiguration config;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;

        public TokenService(
            IConfiguration config,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            this.config = config;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<string> GenerateJsonWebTokenAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User id must be provided.", nameof(userId));
            }

            var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"User '{userId}' was not found.");

            string jwtKey = config["Jwt:Key"];
            string jwtIssuer = config["Jwt:Issuer"];
            string jwtAudience = config["Jwt:Audience"] ?? jwtIssuer;

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("Jwt:Key is not configured.");
            }
            if (string.IsNullOrWhiteSpace(jwtIssuer))
            {
                throw new InvalidOperationException("Jwt:Issuer is not configured.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claimsIdentity = new ClaimsIdentity("Identity.Application", ClaimTypes.Name, ClaimTypes.Role);
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.UserName ?? string.Empty));

            // Unique token id so tokens can be individually tracked or revoked later.
            claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")));

            if (userManager.SupportsUserEmail)
            {
                string email = await userManager.GetEmailAsync(user).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(email))
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, email));
                }
            }

            if (userManager.SupportsUserSecurityStamp)
            {
                const string securityStampClaimType = "AspNet.Identity.SecurityStamp";
                claimsIdentity.AddClaim(new Claim(
                    securityStampClaimType,
                    await userManager.GetSecurityStampAsync(user).ConfigureAwait(false)));
            }

            if (userManager.SupportsUserClaim)
            {
                claimsIdentity.AddClaims(await userManager.GetClaimsAsync(user).ConfigureAwait(false));
            }

            // Include roles so [Authorize(Roles="...")] works against the API, plus any claims
            // assigned to those roles (e.g. "Permission=SettingsRead") so policy-based
            // authorization sees them without us having to duplicate claims onto every user.
            if (userManager.SupportsUserRole)
            {
                var roleNames = await userManager.GetRolesAsync(user).ConfigureAwait(false);
                foreach (var roleName in roleNames)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleName));

                    var role = await roleManager.FindByNameAsync(roleName).ConfigureAwait(false);
                    if (role is not null)
                    {
                        var roleClaims = await roleManager.GetClaimsAsync(role).ConfigureAwait(false);
                        foreach (var roleClaim in roleClaims)
                        {
                            // Avoid duplicating a claim the user already has directly.
                            if (!claimsIdentity.HasClaim(roleClaim.Type, roleClaim.Value))
                            {
                                claimsIdentity.AddClaim(roleClaim);
                            }
                        }
                    }
                }
            }

            var now = DateTime.UtcNow;
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claimsIdentity.Claims,
                notBefore: now,
                expires: now.Add(TokenLifetime),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
