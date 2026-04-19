using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Inferno.Web.Identity;
using InfernoCMS.Api.Models;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace InfernoCMS.Api.Controllers;

[ApiController]
[Route("auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration configuration;
    private readonly ITokenService tokenService;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly ILogger<AuthController> logger;

    public AuthController(
        IConfiguration configuration,
        ITokenService tokenService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AuthController> logger)
    {
        this.configuration = configuration;
        this.tokenService = tokenService;
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.logger = logger;
    }

    /// <summary>
    /// Exchanges API-client credentials + user credentials for a JWT bearer token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] ApiLoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning(
                "API login attempt with invalid model state from {RemoteIpAddress}",
                HttpContext.Connection.RemoteIpAddress);
            return BadRequest(ModelState);
        }

        if (!IsValidApiClient(request.ApiKey))
        {
            logger.LogWarning(
                "API login attempt with invalid API key from {RemoteIpAddress}",
                HttpContext.Connection.RemoteIpAddress);
            return Unauthorized(new { error = "Invalid credentials" });
        }

        var user = await userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            // Intentionally the same response as invalid-password to avoid username
            // enumeration.
            logger.LogWarning(
                "API login attempt with unknown username '{Username}' from {RemoteIpAddress}",
                request.Username, HttpContext.Connection.RemoteIpAddress);
            return Unauthorized(new { error = "Invalid credentials" });
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            logger.LogWarning(
                "Failed API login for user '{Username}' from {RemoteIpAddress}. LockedOut={LockedOut} NotAllowed={NotAllowed}",
                request.Username,
                HttpContext.Connection.RemoteIpAddress,
                result.IsLockedOut,
                result.IsNotAllowed);

            if (result.IsLockedOut)
            {
                return Unauthorized(new { error = "Account is locked out" });
            }
            if (result.IsNotAllowed)
            {
                return Unauthorized(new { error = "Account is not allowed to sign in" });
            }

            return Unauthorized(new { error = "Invalid credentials" });
        }

        try
        {
            var response = await BuildTokenResponseAsync(user);
            logger.LogInformation(
                "Successful API login for user '{Username}' from {RemoteIpAddress}",
                request.Username, HttpContext.Connection.RemoteIpAddress);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate token for user '{Username}'", request.Username);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to generate authentication token" });
        }
    }

    /// <summary>
    /// Refreshes a JWT bearer token. The caller must present their existing (optionally
    /// expired) JWT so that the server can prove the caller once possessed valid credentials.
    /// The API key alone is not sufficient — that would let any API-key holder mint tokens
    /// for arbitrary users.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!IsValidApiClient(request.ApiKey))
        {
            logger.LogWarning(
                "API refresh attempt with invalid API key from {RemoteIpAddress}",
                HttpContext.Connection.RemoteIpAddress);
            return Unauthorized(new { error = "Invalid credentials" });
        }

        ClaimsPrincipal principal;
        try
        {
            principal = ValidateTokenIgnoringLifetime(request.Token);
        }
        catch (SecurityTokenException ex)
        {
            logger.LogWarning(
                ex,
                "API refresh attempt with invalid token from {RemoteIpAddress}",
                HttpContext.Connection.RemoteIpAddress);
            return Unauthorized(new { error = "Invalid token" });
        }

        string userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "Invalid token" });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized(new { error = "User no longer exists" });
        }

        // Reject refresh if the account is currently locked out.
        if (await userManager.IsLockedOutAsync(user))
        {
            logger.LogWarning(
                "API refresh attempt for locked-out user '{UserId}' from {RemoteIpAddress}",
                user.Id, HttpContext.Connection.RemoteIpAddress);
            return Unauthorized(new { error = "Account is locked out" });
        }

        // If the security stamp has rotated (password change, sign-out-everywhere, etc.)
        // refuse the refresh so old tokens cannot be rolled forward.
        if (userManager.SupportsUserSecurityStamp)
        {
            string tokenStamp = principal.FindFirstValue("AspNet.Identity.SecurityStamp");
            string currentStamp = await userManager.GetSecurityStampAsync(user);
            if (!string.IsNullOrEmpty(currentStamp)
                && !string.Equals(tokenStamp, currentStamp, StringComparison.Ordinal))
            {
                logger.LogWarning(
                    "API refresh attempt for user '{UserId}' with stale security stamp from {RemoteIpAddress}",
                    user.Id, HttpContext.Connection.RemoteIpAddress);
                return Unauthorized(new { error = "Token is no longer valid" });
            }
        }

        try
        {
            var response = await BuildTokenResponseAsync(user);
            logger.LogInformation(
                "Refreshed token for user '{UserId}' from {RemoteIpAddress}",
                user.Id, HttpContext.Connection.RemoteIpAddress);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to refresh token for user '{UserId}'", user.Id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to generate authentication token" });
        }
    }

    /// <summary>
    /// Returns information about the authenticated caller. Useful for API clients to
    /// verify their token is valid and to discover the user id associated with it.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me() => Ok(new
    {
        userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
        username = User.Identity?.Name,
        email = User.FindFirstValue(ClaimTypes.Email),
        roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
    });

    private async Task<TokenResponse> BuildTokenResponseAsync(ApplicationUser user)
    {
        string token = await tokenService.GenerateJsonWebTokenAsync(user.Id);
        return new TokenResponse
        {
            Token = token,
            TokenType = "Bearer",
            UserId = user.Id,
            Username = user.UserName,
            Email = user.Email,
            ExpiresAt = DateTime.UtcNow.Add(InfernoCMS.Identity.Services.TokenService.TokenLifetime)
        };
    }

    private ClaimsPrincipal ValidateTokenIgnoringLifetime(string token)
    {
        string jwtKey = configuration["Jwt:Key"];
        string jwtIssuer = configuration["Jwt:Issuer"];
        string jwtAudience = configuration["Jwt:Audience"] ?? jwtIssuer;

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer))
        {
            throw new InvalidOperationException("JWT configuration is missing.");
        }

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // the whole point of refresh is to accept expired tokens
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

        return validatedToken is not JwtSecurityToken jwt
            || !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase)
            ? throw new SecurityTokenException("Invalid token algorithm.")
            : principal;
    }

    private bool IsValidApiClient(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return false;
        }

        string configuredApiKey = configuration.GetValue<string>("ApiKey");
        if (string.IsNullOrEmpty(configuredApiKey))
        {
            logger.LogError("API key is not configured in application settings");
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(apiKey),
            Encoding.UTF8.GetBytes(configuredApiKey));
    }
}