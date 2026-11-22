using System.Globalization;
using Inferno.Tenants.Services;
using Kameleon.Tenancy;

namespace InfernoCMS.Infrastructure;

/// <summary>Lets the host administrator grant a theme to some tenants only, by naming every tenant Kameleon can offer.</summary>
/// <param name="tenantService">Reads the tenants to offer.</param>
public sealed class InfernoThemeTenantDirectory(ITenantService tenantService) : IThemeTenantDirectory
{
    /// <summary>Lists every tenant, ordered by name, with the tenant id formatted as invariant text.</summary>
    /// <param name="cancellationToken">Not observed.</param>
    /// <returns>The tenants. Empty when the host has none.</returns>
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
