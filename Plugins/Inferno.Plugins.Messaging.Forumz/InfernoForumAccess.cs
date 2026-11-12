using System.Security.Claims;
using Forumz.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Plugins.Messaging.Forumz;

/// <summary>
/// Reading stays public. Writing and moderating use Inferno permissions, which already treat
/// an administrator as holding every permission.
/// </summary>
public sealed class InfernoForumAccess(
    IAuthorizationService authorization,
    IHttpContextAccessor httpContextAccessor,
    IServiceProvider services) : IForumAccess
{
    public bool CanRead() => true;

    public bool CanWrite() => Allows(ForumzPolicies.Write);

    public bool CanModerate() => Allows(ForumzPolicies.Moderate);

    public bool CanManage() => Allows(ForumzPolicies.Moderate);

    private bool Allows(string policy)
    {
        var user = Principal();
        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        // The forum runs on its own circuit, where the permission check is synchronous.
        return authorization.AuthorizeAsync(user, policy).GetAwaiter().GetResult().Succeeded;
    }

    private ClaimsPrincipal? Principal()
    {
        var httpUser = httpContextAccessor.HttpContext?.User;
        if (httpUser?.Identity?.IsAuthenticated == true)
        {
            return httpUser;
        }

        var provider = services.GetService<AuthenticationStateProvider>();
        if (provider is null)
        {
            return httpUser;
        }

        try
        {
            var user = Task.Run(async () => (await provider.GetAuthenticationStateAsync().ConfigureAwait(false)).User)
                .GetAwaiter()
                .GetResult();
            return user.Identity?.IsAuthenticated == true ? user : httpUser;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
        {
            return httpUser;
        }
    }
}
