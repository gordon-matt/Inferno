using Dependo;
using Localize;
using Inferno.Security.Membership;
using InfernoCMS.Identity.Services;

namespace InfernoCMS.Infrastructure;

public class DependencyRegistrar : IDependencyRegistrar
{
    public int Order => 1;

    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
        builder.Register<IDbContextFactory, InfernoCMS.Services.DataSettingsDbContextFactory>(ServiceLifetime.Singleton);
        builder.RegisterGeneric(typeof(IRepository<>), typeof(EntityFrameworkRepository<>), ServiceLifetime.Scoped);

        // Services
        builder.Register<IMembershipService, MembershipService>(ServiceLifetime.Transient);
        builder.Register<Inferno.Web.Tenants.ITenantDataCleaner, InfernoCMS.Services.TenantDataCleaner>(ServiceLifetime.Scoped);

        // Localization
        builder.Register<ILanguagePack, LanguagePackInvariant>(ServiceLifetime.Transient);
    }
}