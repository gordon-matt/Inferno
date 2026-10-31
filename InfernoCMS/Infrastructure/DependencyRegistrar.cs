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
        builder.Register<IDbContextFactory, InfernoCMS.Services.DataSettingsDbContextFactory>(ServiceLifetime.Singleton);
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
        builder.Register<Inferno.Web.Tenants.ITenantDataCleaner, InfernoCMS.Services.TenantDataCleaner>(ServiceLifetime.Scoped);
        builder.Register<ITokenService, TokenService>(ServiceLifetime.Scoped);

        // Localization
        builder.Register<ILanguagePack, LanguagePackInvariant>(ServiceLifetime.Transient);
    }
}