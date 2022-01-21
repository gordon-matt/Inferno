using Autofac;
using Dependo.Autofac;
using Inferno.Tasks;

namespace Inferno.Caching.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        #region IDependencyRegistrar Members

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<MemoryCacheManager>().As<ICacheManager>().Named<ICacheManager>("Inferno_Cache_Static").SingleInstance();
            builder.RegisterType<ClearCacheTask>().As<ITask>().SingleInstance();
        }

        public int Order => 0;

        #endregion IDependencyRegistrar Members
    }
}