using Autofac;
using Dependo;
using Dependo.Autofac;
using Localize;
using Inferno.Web.Areas.Admin;
using Inferno.Web.Configuration;
using Inferno.Web.Configuration.Services;
using Inferno.Web.Mvc.Themes;
using Inferno.Web.Navigation;
using Inferno.Web.Scheduling;
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
        builder.Register<Security.Permissions.IPermissionProvider, Security.Permissions.InfernoWebPermissionProvider>(ServiceLifetime.Singleton);

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

        // Admin screens
        builder.Register<Areas.Admin.Tenants.Services.ITenantAdminService, Areas.Admin.Tenants.Services.TenantAdminService>(ServiceLifetime.Scoped);
        builder.Register<Areas.Admin.Configuration.Services.ISettingsAdminService, Areas.Admin.Configuration.Services.SettingsAdminService>(ServiceLifetime.Scoped);
        builder.Register<IScheduledJobService, ScheduledJobService>(ServiceLifetime.Scoped);
        builder.Register<Areas.Admin.Configuration.Themes.Services.IThemeAdminService, Areas.Admin.Configuration.Themes.Services.ThemeAdminService>(ServiceLifetime.Scoped);

        //// Embedded File Provider
        //builder.Register<IEmbeddedFileProviderRegistrar, EmbeddedFileProviderRegistrar>(ServiceLifetime.Scoped);
    }

    public void Register(ContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration) =>
        builder.RegisterModule<ConfigurationModule>();

    public int Order => 0;

    #endregion IDependencyRegistrar Members
}