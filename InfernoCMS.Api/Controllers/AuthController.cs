using Inferno.Web.Identity;
using Inferno.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfernoCMS.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ITokenService tokenService;

        public AuthController(IConfiguration configuration, ITokenService tokenService)
        {
            this.configuration = configuration;
            this.tokenService = tokenService;
        }

        // TODO: Should probably hash the user ID.. otherwise someone can post a userID and use the API as someone else
        [AllowAnonymous]
        [HttpPost]
        [Route("authorize")]
        public async Task<IActionResult> Authorize([FromBody] LoginModel model)
        {
            IActionResult response = Unauthorized();
            var authenticateResult = Authenticate(model);

            if (authenticateResult.Succeeded)
            {
                string token = await tokenService.GenerateJsonWebTokenAsync(model.UserId);
                response = Ok(new { token });
            }

            return response;
        }

        private StandardJsonResult Authenticate(LoginModel auth) => auth.ApiKey == configuration.GetValue<string>("ApiKey")
            ? StandardJsonResult.Success()
            : StandardJsonResult.Failure("Validation Error");
    }
}