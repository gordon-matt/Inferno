using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Inferno.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace InfernoCMS.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration config;

        public AuthController(IConfiguration config) => this.config = config;

        [AllowAnonymous]
        [HttpPost]
        [Route("authorize")]
        public IActionResult Authorize([FromBody] AuthModel auth)
        {
            IActionResult response = Unauthorized();
            var authenticateResult = Authenticate(auth);

            if (authenticateResult.Succeeded)
            {
                string token = GenerateJSONWebToken();
                response = Ok(new { token });
            }

            return response;
        }

        private string GenerateJSONWebToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                config["Jwt:Issuer"],
                config["Jwt:Issuer"],
                null,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private StandardJsonResult Authenticate(AuthModel auth) => auth.ApiKey == config.GetValue<string>("ApiKey")
            ? StandardJsonResult.Success()
            : StandardJsonResult.Failure("Validation Error");
    }
}