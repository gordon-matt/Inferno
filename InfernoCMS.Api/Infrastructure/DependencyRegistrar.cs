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
        builder.Register<IDbContextFactory, ApplicationDbContextFactory>(ServiceLifetime.Singleton);

        builder.RegisterGeneric(typeof(IRepository<>), typeof(EntityFrameworkRepository<>), ServiceLifetime.Scoped);

        builder.Register<IODataRegistrar, ODataRegistrar>(ServiceLifetime.Singleton);

        builder.Register<IMembershipService, MembershipService>(ServiceLifetime.Transient);
    }
}