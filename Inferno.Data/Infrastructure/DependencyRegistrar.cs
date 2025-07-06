using System.Reflection;
using Dependo;
using Inferno.Data.Entity;
using Microsoft.Extensions.Configuration;

namespace Inferno.Data.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        #region IDependencyRegistrar Members

        public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
        {
            var entityTypeConfigurations = typeFinder
                .FindClassesOfType(typeof(IInfernoEntityTypeConfiguration))
                .ToHashSet();

            foreach (var entityTypeConfiguration in entityTypeConfigurations)
            {
                if (entityTypeConfiguration.GetTypeInfo().IsGenericType)
                {
                    continue;
                }

                var isEnabled = (Activator.CreateInstance(entityTypeConfiguration) as IInfernoEntityTypeConfiguration).IsEnabled;

                if (isEnabled)
                {
                    builder.RegisterInstance(typeof(IInfernoEntityTypeConfiguration), entityTypeConfiguration);
                }
            }
        }

        public int Order => 0;

        #endregion IDependencyRegistrar Members
    }
}