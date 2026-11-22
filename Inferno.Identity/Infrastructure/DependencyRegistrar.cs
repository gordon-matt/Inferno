using Dependo;
using Localize;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Identity.Infrastructure;

/// <inheritdoc cref="IDependencyRegistrar" />
public class DependencyRegistrar : IDependencyRegistrar
{
    #region IDependencyRegistrar Members

    /// <inheritdoc />
    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration) =>
        // localization
        builder.Register<ILanguagePack, LanguagePackInvariant>(ServiceLifetime.Singleton);

    /// <inheritdoc />
    public int Order => 0;

    #endregion IDependencyRegistrar Members
}