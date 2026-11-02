using System.Security.Claims;
using Dependo;
using Flatmate;
using Inferno.Tenants.Entities;
using Microsoft.AspNetCore.Http;
using Pluginz.Models;

namespace Inferno.Web.Plugins;

/// <summary>
/// The tenant and roles Pluginz uses when a plugin is limited to some of them. A missing tenant is the global
/// scope, which sees every plugin.
/// </summary>
public sealed class InfernoPluginScope(IHttpContextAccessor httpContextAccessor) : IPluginScope
{
    public string? TenantId
    {
        get
        {
            int? id = CurrentTenant();
            return id is null or 0 ? null : id.Value.ToString();
        }
    }

    public IReadOnlyCollection<string> Roles
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return [];
            }

            return user.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    private static int? CurrentTenant()
    {
        try
        {
            var current = DependoResolver.Instance.Resolve<ICurrentTenant<Tenant>>();
            if (current?.Tenant != null)
            {
                return current.Tenant.Id;
            }
        }
        catch (Exception)
        {
            // A host that has not registered Flatmate still resolves the tenant from the work context.
        }

        try
        {
            return DependoResolver.Instance.Resolve<IWorkContext>().CurrentTenant?.Id;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
