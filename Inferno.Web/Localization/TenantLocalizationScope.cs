using Dependo;
using Flatmate;
using Inferno.Tenants.Entities;
using Localize;

namespace Inferno.Web.Localization;

/// <summary>One localization catalog per tenant. The scope id is the tenant id as text.</summary>
public sealed class TenantLocalizationScope : ILocalizationScope
{
    public string? GetScopeId()
    {
        try
        {
            var current = DependoResolver.Instance.Resolve<ICurrentTenant<Tenant>>();
            if (current?.Tenant != null)
            {
                return current.Tenant.Id.ToString();
            }
        }
        catch (Exception)
        {
            // A host that has not registered Flatmate still resolves the tenant from the work context.
        }

        try
        {
            return DependoResolver.Instance.Resolve<IWorkContext>().CurrentTenant?.Id.ToString();
        }
        catch (Exception)
        {
            return null;
        }
    }
}
