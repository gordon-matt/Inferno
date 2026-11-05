using System.Globalization;
using Inferno.Tenants.Services;
using Kameleon.Tenancy;

namespace InfernoCMS.Infrastructure;

/// <summary>Lets the host administrator grant a theme to some tenants only, by naming every tenant Kameleon can offer.</summary>
public sealed class InfernoThemeTenantDirectory(ITenantService tenantService) : IThemeTenantDirectory
{
    public Task<IReadOnlyList<ThemeTenant>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var connection = tenantService.OpenConnection();

        IReadOnlyList<ThemeTenant> tenants = connection.Query()
            .OrderBy(tenant => tenant.Name)
            .Select(tenant => new { tenant.Id, tenant.Name })
            .ToList()
            .Select(tenant => new ThemeTenant(tenant.Id.ToString(CultureInfo.InvariantCulture), tenant.Name))
            .ToList();

        return Task.FromResult(tenants);
    }
}
