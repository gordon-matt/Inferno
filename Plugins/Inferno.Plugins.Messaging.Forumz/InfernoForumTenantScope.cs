using Forumz.Services;
using Inferno.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Plugins.Messaging.Forumz;

/// <summary>
/// The tenant Flatmate resolved for this circuit. Null stores rows with an empty tenant id,
/// the same as a host that is not multi-tenant.
/// </summary>
public sealed class InfernoForumTenantScope(IServiceProvider services) : IForumTenantScope
{
    public string? TenantId
    {
        get
        {
            int? id = CurrentTenantId.Resolve(services);
            return id?.ToString();
        }
    }
}
