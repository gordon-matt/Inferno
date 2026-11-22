using Inferno.Data.Services;
using Inferno.Tenants.Entities;

namespace Inferno.Tenants.Services;

/// <summary>Reads and writes <see cref="Tenant"/> rows through the generic data service.</summary>
public interface ITenantService : IGenericDataService<Tenant>
{
}