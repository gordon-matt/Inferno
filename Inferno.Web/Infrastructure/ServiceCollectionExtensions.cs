using Inferno.Web.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Web.Infrastructure;

/// <summary>Options binding for Inferno.Web.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Binds Inferno's options from configuration, then lets the host set what configuration cannot hold,
    /// such as <see cref="InfernoWebOptions.PublicLayout"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Values bound onto a new <see cref="InfernoWebOptions"/>.</param>
    /// <param name="configure">Changes applied after binding. <see langword="null"/> leaves the bound values as they are.</param>
    /// <returns>The same collection, so calls can be chained.</returns>
    public static IServiceCollection ConfigureInferno(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<InfernoWebOptions> configure = null)
    {
        var webOptions = new InfernoWebOptions();
        configuration.Bind(webOptions);
        configure?.Invoke(webOptions);
        services.AddSingleton(webOptions);

        return services;
    }
}
