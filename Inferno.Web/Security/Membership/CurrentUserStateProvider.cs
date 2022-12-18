using Inferno.Security.Membership;
using Inferno.Threading;
using Microsoft.AspNetCore.Http;

namespace Inferno.Web.Security.Membership
{
    public class CurrentUserStateProvider : IWorkContextStateProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMembershipService membershipService;

        public CurrentUserStateProvider(
            IHttpContextAccessor httpContextAccessor,
            IMembershipService membershipService)
        {
            this.membershipService = membershipService;
            this.httpContextAccessor = httpContextAccessor;
        }

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

                        if (user == null)
                        {
                            return default;
                        }

                        return (T)(object)user;
                    };
                }
            }
            return null;
        }
    }
}