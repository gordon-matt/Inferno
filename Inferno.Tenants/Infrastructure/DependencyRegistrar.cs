using Autofac;
using Dependo.Autofac;
using Inferno.Tenants.Services;

namespace Inferno.Tenants.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        #region IDependencyRegistrar Members

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<TenantService>().As<ITenantService>().InstancePerDependency();
        }

        public int Order => 0;

        #endregion IDependencyRegistrar Members
    }
}