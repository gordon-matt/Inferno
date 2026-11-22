using System.Security.Claims;
using Inferno.Web.Security.Permissions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace InfernoCMS.Areas.Identity;

/// <summary>
/// Revalidates the Identity security stamp and loads permission grants before the circuit renders.
/// </summary>
/// <typeparam name="TUser">The Identity user type.</typeparam>
public class RevalidatingIdentityAuthenticationStateProvider<TUser>
    : RevalidatingServerAuthenticationStateProvider where TUser : class
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IdentityOptions _options;

    /// <summary>Creates a provider that revalidates users of type <typeparamref name="TUser"/>.</summary>
    /// <param name="loggerFactory">Passed to the base provider.</param>
    /// <param name="scopeFactory">Opens a scope for the user manager and the permission store.</param>
    /// <param name="optionsAccessor">Supplies the security-stamp claim type.</param>
    public RevalidatingIdentityAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<IdentityOptions> optionsAccessor)
        : base(loggerFactory)
    {
        _scopeFactory = scopeFactory;
        _options = optionsAccessor.Value;
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    /// <summary>
    /// The admin menu decides what to show while it renders, so the grants have to be cached
    /// before the shell is drawn. This runs before any page in the circuit.
    /// </summary>
    /// <returns>The authentication state, after the user's grants have been cached when the principal has a user id.</returns>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        AuthenticationState state = await base.GetAuthenticationStateAsync();
        await WarmPermissionsAsync(state.User);
        return state;
    }

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        // Get the user manager from a new scope to ensure it fetches fresh data
        var scope = _scopeFactory.CreateScope();
        try
        {
            using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TUser>>();
            return await ValidateSecurityStampAsync(userManager, authenticationState.User);
        }
        finally
        {
            if (scope is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else
            {
                scope.Dispose();
            }
        }
    }

    private async Task WarmPermissionsAsync(ClaimsPrincipal user)
    {
        string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IPermissionStore>();
        await store.GetGrantedAsync(userId);
    }

    private async Task<bool> ValidateSecurityStampAsync(UserManager<TUser> userManager, ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user == null)
        {
            return false;
        }
        else if (!userManager.SupportsUserSecurityStamp)
        {
            return true;
        }
        else
        {
            string principalStamp = principal.FindFirstValue(_options.ClaimsIdentity.SecurityStampClaimType);
            string userStamp = await userManager.GetSecurityStampAsync(user);
            return principalStamp == userStamp;
        }
    }
}