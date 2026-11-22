using System.Threading;
using Dependo;
using Flatmate;
using Inferno.Tenants.Entities;
using Inferno.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Identity;

/// <summary>
/// The tenant for the current request. Flatmate's circuit copy wins, because the Identity
/// management UI runs on its own Blazor circuit where the original request is gone.
/// </summary>
public static class CurrentTenantId
{
    private static readonly AsyncLocal<IServiceProvider> Ambient = new();

    /// <summary>
    /// Makes <paramref name="services"/> the scope tenant lookups use until the result is disposed.
    /// The circuit handler calls this, because a circuit's services are not the root container.
    /// </summary>
    /// <param name="services">The scope whose tenant is visible until the result is disposed.</param>
    /// <returns>A handle that restores the previous scope when disposed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IDisposable Use(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var previous = Ambient.Value;
        Ambient.Value = services;
        return new Restore(previous);
    }

    /// <summary>
    /// The current tenant, or null when this scope has none. Startup and a circuit with no request
    /// are both in that position, and guessing a tenant would show the wrong site.
    /// </summary>
    /// <returns>The current tenant id, or <see langword="null"/> when the scope has none.</returns>
    public static int? Resolve()
    {
        var services = Ambient.Value;
        if (services is null)
        {
            try
            {
                services = DependoResolver.Instance.Resolve<IHttpContextAccessor>().HttpContext?.RequestServices;
            }
            catch (Exception)
            {
                // Before the host has built the container there is no request to read.
                services = null;
            }
        }

        if (services is null)
        {
            try
            {
                services = DependoResolver.Instance.Resolve<IServiceProvider>();
            }
            catch (Exception)
            {
                return null;
            }
        }

        return Resolve(services);
    }

    /// <summary>The tenant visible to <paramref name="services"/>, or null when that scope has none.</summary>
    /// <param name="services">The scope to read.</param>
    /// <returns>The tenant id visible to <paramref name="services"/>, or <see langword="null"/> when that scope has none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static int? Resolve(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        try
        {
            if (services.GetService<ICurrentTenant<Tenant>>()?.Tenant is { } tenant)
            {
                return tenant.Id;
            }
        }
        catch (Exception)
        {
            // A host that has not registered Flatmate still resolves the tenant from the work context.
        }

        try
        {
            return services.GetService<IWorkContext>()?.CurrentTenant?.Id;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private sealed class Restore(IServiceProvider previous) : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            Ambient.Value = previous;
        }
    }
}
