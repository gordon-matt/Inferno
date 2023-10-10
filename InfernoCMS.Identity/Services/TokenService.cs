﻿using Inferno.Web.Identity;
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
        private readonly IConfiguration config;
        private readonly UserManager<ApplicationUser> userManager;

        public TokenService(IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            this.config = config;
            this.userManager = userManager;
        }

        public async Task<string> GenerateJsonWebTokenAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claimsIdentity = new ClaimsIdentity("Identity.Application", ClaimTypes.Name, ClaimTypes.Role);
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            if (userManager.SupportsUserEmail)
            {
                string email = await userManager.GetEmailAsync(user).ConfigureAwait(continueOnCapturedContext: false);
                if (!string.IsNullOrEmpty(email))
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, email));
                }
            }

            if (userManager.SupportsUserSecurityStamp)
            {
                string securityStampClaimType = "AspNet.Identity.SecurityStamp";
                claimsIdentity.AddClaim(new Claim(securityStampClaimType, await userManager.GetSecurityStampAsync(user).ConfigureAwait(continueOnCapturedContext: false)));
            }

            if (userManager.SupportsUserClaim)
            {
                claimsIdentity.AddClaims(await userManager.GetClaimsAsync(user).ConfigureAwait(continueOnCapturedContext: false));
            }

            var token = new JwtSecurityToken(
                config["Jwt:Issuer"],
                config["Jwt:Issuer"],
                claimsIdentity.Claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}