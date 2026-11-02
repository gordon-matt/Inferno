using Autofac;
using Dependo;
using Dependo.Autofac;
using Extenso.AspNetCore.OData;
using Localize;
using Inferno.Plugins;
using Inferno.Tasks.Entities;
using Inferno.Tenants.Entities;
using Inferno.Web.Areas.Admin;
using Inferno.Web.Areas.Admin.Configuration.Services;
using Inferno.Web.Areas.Admin.Configuration.Themes.Models;
using Inferno.Web.Areas.Admin.Configuration.Themes.Services;
using Inferno.Web.Areas.Admin.Plugins.Models;
using Inferno.Web.Areas.Admin.Plugins.Services;
using Inferno.Web.Areas.Admin.ScheduledTasks.Services;
using Inferno.Web.Areas.Tenants.Services;
using Inferno.Web.Configuration;
using Inferno.Web.Configuration.Entities;
using Inferno.Web.Configuration.Services;
using Inferno.Web.Mvc.Themes;
using Inferno.Web.Navigation;
using Inferno.Web.OData;
using Inferno.Web.Security.Membership;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Web.Infrastructure;

public class DependencyRegistrar : IDependencyRegistrar, IAutofacDependencyRegistrar
{
    #region IDependencyRegistrar Members

    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
        //var settings = DataSettingsManager.LoadSettings();
        //builder.RegisterInstance(settings);
        builder.RegisterInstance(DataSettingsManager.LoadSettings());

        builder.Register<IRouterAssemblyMarker, RouterAssemblyMarker>(ServiceLifetime.Singleton);

        // Helpers
        builder.Register<IWebHelper, WebHelper>(ServiceLifetime.Scoped);
        //builder.Register<IDateTimeHelper, DateTimeHelper>(ServiceLifetime.Scoped);

        // Plugins
        builder.Register<IPluginFinder, PluginFinder>(ServiceLifetime.Scoped);

        // Work Context, Themes, Routing, etc
        builder.Register<IWorkContext, WorkContext>(ServiceLifetime.Scoped);
        builder.Register<IThemeProvider, ThemeProvider>(ServiceLifetime.Scoped);
        builder.Register<IThemeContext, ThemeContext>(ServiceLifetime.Scoped);
        //builder.Register<IEmbeddedResourceResolver, EmbeddedResourceResolver>(ServiceLifetime.Singleton);
        //builder.Register<IRoutePublisher, RoutePublisher>(ServiceLifetime.Singleton);

        //// Resources (JS and CSS)
        //builder.Register<ScriptRegistrar>().AsSelf().InstancePerLifetimeScope();
        //builder.Register<StyleRegistrar>().AsSelf().InstancePerLifetimeScope();
        //builder.Register<ResourcesManager>().As<IResourcesManager>().InstancePerLifetimeScope();

        // Security
        //builder.Register<RolesBasedAuthorizationService>().As<IAuthorizationService>().SingleInstance();

        // Configuration
        builder.Register<ISettingService, DefaultSettingService>(ServiceLifetime.Transient);
        builder.Register<ISettings, SiteSettings>(ServiceLifetime.Scoped);
        builder.Register<ISettings, MembershipSettings>(ServiceLifetime.Scoped);
        builder.Register<ISettings, Messaging.Configuration.SmtpSettings>(ServiceLifetime.Scoped);

        builder.Register<Events.IEventPublisher, Events.EventPublisher>(ServiceLifetime.Scoped);
        builder.Register<Events.ISubscriptionService, Events.SubscriptionService>(ServiceLifetime.Scoped);
        builder.Register<Messaging.Services.IEmailSender, Messaging.Services.DefaultEmailSender>(ServiceLifetime.Scoped);
        builder.Register<Messaging.ITokenizer, Messaging.Tokenizer>(ServiceLifetime.Singleton);
        builder.Register<Messaging.Services.IMessageTemplateService, Messaging.Services.MessageTemplateService>(ServiceLifetime.Transient);
        builder.Register<Messaging.Services.IMessageTemplateVersionService, Messaging.Services.MessageTemplateVersionService>(ServiceLifetime.Transient);
        builder.Register<Messaging.Services.IMessageService, Messaging.Services.MessageService>(ServiceLifetime.Transient);
        builder.Register<Messaging.IQueuedMessageProvider, Messaging.Services.MessageService>(ServiceLifetime.Transient);
        builder.Register<Tasks.ITask, Messaging.ProcessQueuedMailTask>(ServiceLifetime.Singleton);

        // Navigation
        builder.Register<INavigationManager, NavigationManager>(ServiceLifetime.Transient);
        builder.Register<INavigationProvider, AdminNavigationProvider>(ServiceLifetime.Singleton);

        // Work Context State Providers
        builder.Register<IWorkContextStateProvider, CurrentUserStateProvider>(ServiceLifetime.Transient);
        builder.Register<IWorkContextStateProvider, CurrentThemeStateProvider>(ServiceLifetime.Transient);
        builder.Register<IWorkContextStateProvider, Localization.CurrentCultureCodeStateProvider>(ServiceLifetime.Transient);

        builder.Register<Localization.IWebCultureManager, Localization.WebCultureManager>(ServiceLifetime.Scoped);
        builder.Register<Localization.Services.ICultureSelector, Localization.Services.CookieCultureSelector>(ServiceLifetime.Singleton);

        // Localization
        builder.Register<ILanguagePack, LanguagePackInvariant>(ServiceLifetime.Transient);
        //builder.Register<IWebCultureManager, WebCultureManager>(ServiceLifetime.Scoped);
        //builder.Register<ICultureSelector, SiteCultureSelector>(ServiceLifetime.Singleton);
        //builder.Register<ICultureSelector, CookieCultureSelector>(ServiceLifetime.Singleton);

        // User Profile Providers
        builder.Register<IUserProfileProvider, AccountUserProfileProvider>(ServiceLifetime.Singleton);
        builder.Register<IUserProfileProvider, ThemeUserProfileProvider>(ServiceLifetime.Singleton);

        //// Data / Services
        //builder.Register<IGenericAttributeService, GenericAttributeService>(ServiceLifetime.Scoped);

        //// Rendering
        //builder.Register<IRazorViewRenderService, MantleRazorViewRenderService>(ServiceLifetime.Transient);

        builder.Register<IODataRegistrar, ODataRegistrar>(ServiceLifetime.Singleton);

        //// Embedded File Provider
        //builder.Register<IEmbeddedFileProviderRegistrar, EmbeddedFileProviderRegistrar>(ServiceLifetime.Scoped);

        builder.Register<IRadzenODataService<ScheduledTask, int>, ScheduledTaskODataService>(ServiceLifetime.Singleton);
        builder.RegisterSelf<ScheduledTaskODataService>(ServiceLifetime.Singleton);
        builder.Register<IRadzenODataService<EdmPluginDescriptor, string>, PluginODataService>(ServiceLifetime.Singleton);
        builder.RegisterSelf<PluginODataService>(ServiceLifetime.Singleton);

        builder.Register<IRadzenODataService<Setting, Guid>, SettingODataService>(ServiceLifetime.Singleton);
        builder.Register<IRadzenODataService<Tenant, int>, TenantODataService>(ServiceLifetime.Singleton);
        builder.Register<IRadzenODataService<EdmThemeConfiguration, Guid>, ThemeODataService>(ServiceLifetime.Singleton);
        builder.RegisterSelf<ThemeODataService>(ServiceLifetime.Singleton);
    }

    public void Register(ContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration) =>
        builder.RegisterModule<ConfigurationModule>();

    public int Order => 0;

    #endregion IDependencyRegistrar Members
}