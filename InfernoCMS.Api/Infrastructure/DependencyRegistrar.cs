using Dependo;
using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Inferno.Security.Membership;
using InfernoCMS.Data;
using InfernoCMS.Identity.Services;

namespace InfernoCMS.Api.Infrastructure;

public class DependencyRegistrar : IDependencyRegistrar
{
    public int Order => 1;

    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
        builder.Register<IDbContextFactory, InfernoCMS.Services.DataSettingsDbContextFactory>(ServiceLifetime.Singleton);

        builder.RegisterGeneric(typeof(IRepository<>), typeof(EntityFrameworkRepository<>), ServiceLifetime.Scoped);

        builder.Register<IODataRegistrar, ODataRegistrar>(ServiceLifetime.Singleton);

        builder.Register<IMembershipService, MembershipService>(ServiceLifetime.Transient);
        builder.Register<Inferno.Web.Tenants.ITenantDataCleaner, InfernoCMS.Services.TenantDataCleaner>(ServiceLifetime.Scoped);
    }
}