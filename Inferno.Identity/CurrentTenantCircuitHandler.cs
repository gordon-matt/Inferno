using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Identity;

/// <summary>
/// Points tenant lookups at the circuit for the length of each circuit activity. The circuit keeps
/// Flatmate's copy of the tenant after the HTTP request that opened it has gone.
/// </summary>
public sealed class CurrentTenantCircuitHandler(IServiceProvider services) : CircuitHandler
{
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
    public static IServiceCollection AddCurrentTenantCircuit(this IServiceCollection services)
    {
        services.AddScoped<CircuitHandler, CurrentTenantCircuitHandler>();
        return services;
    }
}
