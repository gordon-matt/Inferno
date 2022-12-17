using Autofac;
using Dependo.Autofac;
using Extenso.AspNetCore.OData;
using Inferno.Localization;
using Inferno.Security.Membership;
using Inferno.Web.OData;
using InfernoCMS.Identity.Services;
using InfernoCMS.Services;
using Radzen;

namespace InfernoCMS.Infrastructure
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

            // Radzen
            builder.RegisterType<DialogService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<NotificationService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<TooltipService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<ContextMenuService>().AsSelf().InstancePerLifetimeScope();

            // Services
            //builder.RegisterGeneric(typeof(GenericODataService<,>))
            //    .As(typeof(IGenericODataService<,>))
            //    .InstancePerLifetimeScope();

            // Services
            builder.RegisterType<MembershipService>().As<IMembershipService>().InstancePerDependency();

            // Localization
            builder.RegisterType<LanguagePackInvariant>().As<ILanguagePack>().InstancePerDependency();

            // Navigation
            //builder.RegisterType<AdminNavigationProvider>().As<INavigationProvider>().SingleInstance();
        }
    }
}