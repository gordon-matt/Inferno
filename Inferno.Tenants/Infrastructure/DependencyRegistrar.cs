using Dependo;
using Inferno.Tenants.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Tenants.Infrastructure;

/// <summary>Registers <see cref="ITenantService"/>.</summary>
public class DependencyRegistrar : IDependencyRegistrar
{
    #region IDependencyRegistrar Members

    /// <summary>Registers <see cref="TenantService"/> as a transient <see cref="ITenantService"/>.</summary>
    /// <param name="builder">The container being filled.</param>
    /// <param name="typeFinder">Not used.</param>
    /// <param name="configuration">Not used.</param>
    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration) =>
        builder.Register<ITenantService, TenantService>(ServiceLifetime.Transient);

    /// <summary>Runs with the other registrars that leave the order at zero.</summary>
    public int Order => 0;

    #endregion IDependencyRegistrar Members
}