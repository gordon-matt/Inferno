using Dependo;
using Inferno.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Identity.Infrastructure;

public class DependencyRegistrar : IDependencyRegistrar
{
    #region IDependencyRegistrar Members

    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration) =>
        // localization
        builder.Register<ILanguagePack, LanguagePackInvariant>(ServiceLifetime.Singleton);

    public int Order => 0;

    #endregion IDependencyRegistrar Members
}