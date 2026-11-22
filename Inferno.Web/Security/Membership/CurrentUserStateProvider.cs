using Inferno.Security.Membership;
using Inferno.Threading;
using Microsoft.AspNetCore.Http;

namespace Inferno.Web.Security.Membership;

/// <summary>
/// Supplies <see cref="InfernoWebConstants.StateProviders.CurrentUser"/> for an authenticated request.
/// The user is loaded for the current tenant, then for no tenant. No request, or an anonymous user, supplies nothing.
/// </summary>
public class CurrentUserStateProvider : IWorkContextStateProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IMembershipService membershipService;

    /// <summary>Creates the provider.</summary>
    /// <param name="httpContextAccessor">The request whose user is read.</param>
    /// <param name="membershipService">Loads the user by name.</param>
    public CurrentUserStateProvider(
        IHttpContextAccessor httpContextAccessor,
        IMembershipService membershipService)
    {
        this.membershipService = membershipService;
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Func<IWorkContext, T> Get<T>(string name)
    {
        if (name == InfernoWebConstants.StateProviders.CurrentUser)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return null;
            }

            if (httpContext.User.Identity.IsAuthenticated)
            {
                return ctx =>
                {
                    httpContext = httpContextAccessor.HttpContext;
                    var user = AsyncHelper.RunSync(() => membershipService.GetUserByNameAsync(ctx.CurrentTenant.Id, httpContext.User.Identity.Name));
                    user ??= AsyncHelper.RunSync(() => membershipService.GetUserByNameAsync(null, httpContext.User.Identity.Name));

                    return user == null ? default : (T)(object)user;
                };
            }
        }
        return null;
    }
}