using Autofac;
using Dependo.Autofac;
using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Inferno.Security.Membership;
using Inferno.Web.OData;
using InfernoCMS.Data;
using InfernoCMS.Data.Entities;
using InfernoCMS.Identity.Services;
using InfernoCMS.Services;

namespace InfernoCMS.Api.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 1;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<ApplicationDbContextFactory>().As<IDbContextFactory>().SingleInstance();

            builder.RegisterGeneric(typeof(EntityFrameworkRepository<>))
                .As(typeof(IRepository<>))
                .InstancePerLifetimeScope();

            builder.RegisterType<ODataRegistrar>().As<IODataRegistrar>().SingleInstance();

            // Services
            builder.RegisterType<MembershipService>().As<IMembershipService>().InstancePerDependency();
            builder.RegisterType<PersonODataService>().As<IRadzenODataService<Person, int>>().SingleInstance();
        }
    }
}