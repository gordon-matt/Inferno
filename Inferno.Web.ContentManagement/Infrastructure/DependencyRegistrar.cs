using Autofac;
using Dependo.Autofac;
using Inferno.Web.Infrastructure;

namespace Inferno.Web.ContentManagement.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        #region IDependencyRegistrar Members

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<RouterAssemblyMarker>().As<IRouterAssemblyMarker>().SingleInstance();
        }

        public int Order => 1;

        #endregion IDependencyRegistrar Members
    }
}