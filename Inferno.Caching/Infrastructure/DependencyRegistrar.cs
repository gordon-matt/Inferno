using Dependo;
using Inferno.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Caching.Infrastructure;

/// <summary>Registers the memory cache and the job that clears it.</summary>
public class DependencyRegistrar : IDependencyRegistrar
{
    #region IDependencyRegistrar Members

    /// <summary>Registers <see cref="MemoryCacheManager"/> as a singleton named Inferno_Cache_Static, and <see cref="ClearCacheTask"/> as a singleton <see cref="ITask"/>.</summary>
    /// <param name="builder">The container being filled.</param>
    /// <param name="typeFinder">Not used.</param>
    /// <param name="configuration">Not used.</param>
    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
        builder.RegisterNamed<ICacheManager, MemoryCacheManager>("Inferno_Cache_Static", ServiceLifetime.Singleton);
        builder.Register<ITask, ClearCacheTask>(ServiceLifetime.Singleton);
    }

    /// <summary>Runs with the other registrars that leave the order at zero.</summary>
    public int Order => 0;

    #endregion IDependencyRegistrar Members
}