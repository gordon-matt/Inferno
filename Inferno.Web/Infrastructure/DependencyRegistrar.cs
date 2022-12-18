using Autofac;
using Dependo.Autofac;
using Extenso.AspNetCore.OData;
using Inferno.Localization;
using Inferno.Security;
using Inferno.Tenants.Entities;
using Inferno.Web.Areas.Admin.Configuration.Services;
using Inferno.Web.Areas.Tenants.Services;
using Inferno.Web.Configuration;
using Inferno.Web.Configuration.Entities;
using Inferno.Web.Configuration.Services;
using Inferno.Web.Mvc.Themes;
using Inferno.Web.Navigation;
using Inferno.Web.OData;
using Inferno.Web.Security.Membership;

namespace Inferno.Web.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        #region IDependencyRegistrar Members

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            //var settings = DataSettingsManager.LoadSettings();
            //builder.Register(x => settings).As<DataSettings>();

            // Helpers
            builder.RegisterType<WebHelper>().As<IWebHelper>().InstancePerLifetimeScope();
            //builder.RegisterType<DateTimeHelper>().As<IDateTimeHelper>().InstancePerLifetimeScope();

            //// Plugins
            //builder.RegisterType<PluginFinder>().As<IPluginFinder>().InstancePerLifetimeScope();

            // Work Context, Themes, Routing, etc
            builder.RegisterType<WorkContext>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<ThemeProvider>().As<IThemeProvider>().InstancePerLifetimeScope();
            //builder.RegisterType<ThemeContext>().As<IThemeContext>().InstancePerLifetimeScope();
            //builder.RegisterType<EmbeddedResourceResolver>().As<IEmbeddedResourceResolver>().SingleInstance();
            //builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().SingleInstance();

            //// Resources (JS and CSS)
            //builder.RegisterType<ScriptRegistrar>().AsSelf().InstancePerLifetimeScope();
            //builder.RegisterType<StyleRegistrar>().AsSelf().InstancePerLifetimeScope();
            ////builder.RegisterType<ResourcesManager>().As<IResourcesManager>().InstancePerLifetimeScope();

            // Security
            //builder.RegisterType<RolesBasedAuthorizationService>().As<IAuthorizationService>().SingleInstance();

            // Configuration
            builder.RegisterModule<ConfigurationModule>();
            builder.RegisterType<DefaultSettingService>().As<ISettingService>();
            builder.RegisterType<SiteSettings>().As<ISettings>().InstancePerLifetimeScope();
            builder.RegisterType<MembershipSettings>().As<ISettings>().InstancePerLifetimeScope();

            // Navigation
            builder.RegisterType<NavigationManager>().As<INavigationManager>().InstancePerDependency();
            //builder.RegisterType<NavigationProvider>().As<INavigationProvider>().SingleInstance();

            // Work Context State Providers
            builder.RegisterType<CurrentUserStateProvider>().As<IWorkContextStateProvider>();
            builder.RegisterType<CurrentThemeStateProvider>().As<IWorkContextStateProvider>();
            //builder.RegisterType<CurrentCultureCodeStateProvider>().As<IWorkContextStateProvider>();

            // Localization
            builder.RegisterType<LanguagePackInvariant>().As<ILanguagePack>().InstancePerDependency();
            //builder.RegisterType<WebCultureManager>().AsImplementedInterfaces().InstancePerLifetimeScope();
            //builder.RegisterType<SiteCultureSelector>().As<ICultureSelector>().SingleInstance();
            //builder.RegisterType<CookieCultureSelector>().As<ICultureSelector>().SingleInstance();

            // User Profile Providers
            builder.RegisterType<AccountUserProfileProvider>().As<IUserProfileProvider>().SingleInstance();
            builder.RegisterType<ThemeUserProfileProvider>().As<IUserProfileProvider>().SingleInstance();

            //// Data / Services
            //builder.RegisterType<GenericAttributeService>().As<IGenericAttributeService>().InstancePerLifetimeScope();

            //// Rendering
            //builder.RegisterType<RazorViewRenderService>().As<IRazorViewRenderService>().SingleInstance();

            builder.RegisterType<ODataRegistrar>().As<IODataRegistrar>().SingleInstance();

            //// Embedded File Provider
            //builder.RegisterType<EmbeddedFileProviderRegistrar>().As<IEmbeddedFileProviderRegistrar>().InstancePerLifetimeScope();

            builder.RegisterType<SettingODataService>().As<IRadzenODataService<Setting, Guid>>().SingleInstance();
            builder.RegisterType<TenantODataService>().As<IRadzenODataService<Tenant, int>>().SingleInstance();
        }

        public int Order => 0;

        #endregion IDependencyRegistrar Members
    }
}