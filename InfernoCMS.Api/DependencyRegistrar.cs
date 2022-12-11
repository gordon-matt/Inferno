using Autofac;
using Dependo.Autofac;
using Extenso.Data.Entity;
using Inferno.Security.Membership;
using Inferno.Web;
using Inferno.Web.Configuration;
using InfernoCMS.Data;
using InfernoCMS.Identity.Services;

namespace InfernoCMS.Api
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

            // Services
            builder.RegisterType<MembershipService>().As<IMembershipService>().InstancePerDependency();
        }
    }
}