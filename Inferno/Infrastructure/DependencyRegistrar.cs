using Autofac;
using Dependo.Autofac;
using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Inferno.Data;
using Inferno.Data.Entities;
using Inferno.Services;
using Radzen;

namespace Inferno.Infrastructure
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

            // Radzen
            builder.RegisterType<DialogService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<NotificationService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<TooltipService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<ContextMenuService>().AsSelf().InstancePerLifetimeScope();

            // Services
            builder.RegisterType<PersonODataService>().As<IGenericODataService<Person, int>>().SingleInstance();
            //builder.RegisterGeneric(typeof(GenericODataService<,>))
            //    .As(typeof(IGenericODataService<,>))
            //    .InstancePerLifetimeScope();
        }
    }
}