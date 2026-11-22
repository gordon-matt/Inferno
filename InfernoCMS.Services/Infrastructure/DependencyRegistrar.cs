using Dependo;
using Microsoft.Extensions.Configuration;

namespace InfernoCMS.Services;

/// <inheritdoc cref="IDependencyRegistrar" />
public class DependencyRegistrar : IDependencyRegistrar
{
    /// <inheritdoc />
    /// <remarks>Returns 999 so this registrar, which adds no services, runs after the others.</remarks>
    public int Order => 999;

    /// <inheritdoc />
    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
    }
}
