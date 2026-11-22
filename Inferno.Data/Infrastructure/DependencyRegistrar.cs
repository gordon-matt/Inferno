using System.Reflection;
using Dependo;
using Inferno.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Data.Infrastructure;

/// <summary>Registers enabled entity maps.</summary>
public class DependencyRegistrar : IDependencyRegistrar
{
    #region IDependencyRegistrar Members

    /// <summary>
    /// Registers each non-generic <see cref="IInfernoEntityTypeConfiguration"/> whose <see cref="IInfernoEntityTypeConfiguration.IsEnabled"/> is <see langword="true"/>, as scoped.
    /// An instance is created to read that flag. Open generic types are skipped.
    /// </summary>
    /// <param name="builder">The container being filled.</param>
    /// <param name="typeFinder">Finds the map types.</param>
    /// <param name="configuration">Not used.</param>
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

            bool isEnabled = (Activator.CreateInstance(entityTypeConfiguration) as IInfernoEntityTypeConfiguration).IsEnabled;

            if (isEnabled)
            {
                builder.Register(typeof(IInfernoEntityTypeConfiguration), entityTypeConfiguration, ServiceLifetime.Scoped);
            }
        }
    }

    /// <summary>Runs with the other registrars that leave the order at zero.</summary>
    public int Order => 0;

    #endregion IDependencyRegistrar Members
}