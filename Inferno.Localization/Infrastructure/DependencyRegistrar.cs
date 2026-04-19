using Dependo;
using Inferno.Localization.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Localization.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        #region IDependencyRegistrar Members

        public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
        {
            builder.Register<ILanguageService, LanguageService>(ServiceLifetime.Transient);
            builder.Register<ILocalizableStringService, LocalizableStringService>(ServiceLifetime.Transient);
            builder.Register<ILocalizablePropertyService, LocalizablePropertyService>(ServiceLifetime.Transient);
        }

        public int Order => 0;

        #endregion IDependencyRegistrar Members
    }
}