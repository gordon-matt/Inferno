using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Identity;

/// <summary>
/// Points tenant lookups at the circuit for the length of each circuit activity. The circuit keeps
/// Flatmate's copy of the tenant after the HTTP request that opened it has gone.
/// </summary>
/// <param name="services">The circuit's service provider, which holds Flatmate's tenant.</param>
public sealed class CurrentTenantCircuitHandler(IServiceProvider services) : CircuitHandler
{
    /// <summary>Wraps each circuit activity so tenant lookups use this circuit's services.</summary>
    /// <param name="next">The activity handler that runs inside the tenant scope.</param>
    /// <returns>A handler that sets the tenant scope, runs <paramref name="next"/>, and then restores the previous scope.</returns>
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next) =>
        async context =>
        {
            using (CurrentTenantId.Use(services))
            {
                await next(context);
            }
        };
}

/// <summary>Registers <see cref="CurrentTenantCircuitHandler"/>.</summary>
public static class CurrentTenantCircuitExtensions
{
    /// <summary>Registers <see cref="CurrentTenantCircuitHandler"/> as a scoped circuit handler.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services"/>, so registration can be chained.</returns>
    public static IServiceCollection AddCurrentTenantCircuit(this IServiceCollection services)
    {
        services.AddScoped<CircuitHandler, CurrentTenantCircuitHandler>();
        return services;
    }
}
