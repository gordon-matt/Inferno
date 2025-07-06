using Dependo;
using Inferno.Localization;
using Inferno.Security.Membership;
using Inferno.Web.Identity;
using InfernoCMS.Identity.Services;
using Radzen;

namespace InfernoCMS.Infrastructure;

public class DependencyRegistrar : IDependencyRegistrar
{
    public int Order => 1;

    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
        builder.Register<IDbContextFactory, ApplicationDbContextFactory>(ServiceLifetime.Singleton);
        builder.RegisterGeneric(typeof(IRepository<>), typeof(EntityFrameworkRepository<>), ServiceLifetime.Scoped);

        // Radzen
        builder.RegisterSelf<DialogService>(ServiceLifetime.Scoped);
        builder.RegisterSelf<NotificationService>(ServiceLifetime.Scoped);
        builder.RegisterSelf<TooltipService>(ServiceLifetime.Scoped);
        builder.RegisterSelf<ContextMenuService>(ServiceLifetime.Scoped);

        // Services
        //builder.RegisterGeneric(typeof(IGenericODataService<>), typeof(GenericODataService<>), ServiceLifetime.Scoped);

        // Services
        builder.Register<IMembershipService, MembershipService>(ServiceLifetime.Transient);
        builder.Register<ITokenService, TokenService>(ServiceLifetime.Scoped);

        // Localization
        builder.Register<ILanguagePack, LanguagePackInvariant>(ServiceLifetime.Transient);
    }
}