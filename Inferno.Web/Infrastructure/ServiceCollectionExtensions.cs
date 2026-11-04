using Inferno.Web.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Web.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Binds Inferno's options from configuration, then lets the host set what configuration cannot hold,
    /// such as <see cref="InfernoWebOptions.PublicLayout"/>.
    /// </summary>
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
