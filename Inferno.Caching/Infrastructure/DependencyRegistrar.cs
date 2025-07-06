using Dependo;
using Inferno.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Caching.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        #region IDependencyRegistrar Members

        public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
        {
            builder.RegisterNamed<ICacheManager, MemoryCacheManager>("Inferno_Cache_Static", ServiceLifetime.Singleton);
            builder.Register<ITask, ClearCacheTask>(ServiceLifetime.Singleton);
        }

        public int Order => 0;

        #endregion IDependencyRegistrar Members
    }
}