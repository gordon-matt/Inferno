using System.Globalization;
using BlogKrafter.Tenancy;
using Inferno.Identity;

namespace InfernoCMS.Infrastructure;

/// <summary>The blog stores the current Inferno tenant as text, because that is the key it indexes on.</summary>
public sealed class InfernoBlogTenantProvider : IBlogTenantProvider
{
    public string GetTenantId()
    {
        int? tenantId = CurrentTenantId.Resolve();
        return tenantId.HasValue ? tenantId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
    }
}
