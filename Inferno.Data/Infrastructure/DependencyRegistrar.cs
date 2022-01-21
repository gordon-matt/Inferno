using System.Reflection;
using Autofac;
using Dependo.Autofac;
using Inferno.Data.Entity;

namespace Inferno.Data.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        #region IDependencyRegistrar Members

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            var entityTypeConfigurations = typeFinder
                .FindClassesOfType(typeof(IInfernoEntityTypeConfiguration))
                .ToHashSet();

            foreach (var configuration in entityTypeConfigurations)
            {
                if (configuration.GetTypeInfo().IsGenericType)
                {
                    continue;
                }

                var isEnabled = (Activator.CreateInstance(configuration) as IInfernoEntityTypeConfiguration).IsEnabled;

                if (isEnabled)
                {
                    builder.RegisterType(configuration).As(typeof(IInfernoEntityTypeConfiguration)).InstancePerLifetimeScope();
                }
            }
        }

        public int Order
        {
            get { return 0; }
        }

        #endregion IDependencyRegistrar Members
    }
}