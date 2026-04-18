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

        public TokenService(IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            this.config = config;
            this.userManager = userManager;
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

            // Include roles so [Authorize(Roles="...")] works against the API.
            if (userManager.SupportsUserRole)
            {
                var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
                foreach (var role in roles)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
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
