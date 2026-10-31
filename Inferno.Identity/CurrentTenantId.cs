using Dependo;
using Flatmate;
using Inferno.Tenants.Entities;
using Inferno.Web;

namespace Inferno.Identity;

/// <summary>
/// The tenant for the current request. Flatmate's circuit copy wins, because the Identity
/// management UI runs on its own Blazor circuit where the original request is gone.
/// </summary>
public static class CurrentTenantId
{
    public static int Resolve()
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
            // A host that has not registered Flatmate still resolves the tenant from the request.
        }

        return DependoResolver.Instance.Resolve<IWorkContext>().CurrentTenant.Id;
    }
}