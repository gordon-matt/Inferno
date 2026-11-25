using Dependo;
using Inferno.Security.Membership;
using Inferno.Web.Infrastructure;
using Inferno.Web.Messaging;
using Inferno.Web.Messaging.Services;
using Inferno.Web.Navigation;
using Inferno.Web.Security.Permissions;
using InfernoCMS.Identity.Services;
using InfernoCMS.Infrastructure.Kourier;
using Localize;

namespace InfernoCMS.Infrastructure;

/// <summary>Registers the CMS database factory, repositories, membership service, tenant cleaner, and invariant language pack.</summary>
public class DependencyRegistrar : IDependencyRegistrar
{
    /// <inheritdoc />
    public int Order => 1;

    /// <inheritdoc />
    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
        builder.Register<IDbContextFactory, InfernoCMS.Services.DataSettingsDbContextFactory>(ServiceLifetime.Singleton);
        builder.RegisterGeneric(typeof(IRepository<>), typeof(EntityFrameworkRepository<>), ServiceLifetime.Scoped);

        // Services
        builder.Register<IMembershipService, MembershipService>(ServiceLifetime.Transient);
        builder.Register<Inferno.Web.Tenants.ITenantDataCleaner, InfernoCMS.Services.TenantDataCleaner>(ServiceLifetime.Scoped);

        // Kourier replaces the old SMTP sender and message queue once installation has saved a connection.
        bool databaseInstalled = !string.IsNullOrWhiteSpace(DataSettingsManager.LoadSettings().ConnectionString);
        if (databaseInstalled)
        {
            builder.Register<IEmailSender, KourierEmailSenderShim>(ServiceLifetime.Scoped);
            builder.Register<IMessageService, KourierMessageServiceShim>(ServiceLifetime.Transient);
            builder.Register<IQueuedMessageProvider, KourierMessageServiceShim>(ServiceLifetime.Transient);
            builder.Register<IPermissionProvider, KourierPermissionProvider>(ServiceLifetime.Singleton);
            builder.Register<INavigationProvider, KourierNavigationProvider>(ServiceLifetime.Singleton);
        }
        else
        {
            builder.Register<IEmailSender, DefaultEmailSender>(ServiceLifetime.Scoped);
            builder.Register<IMessageService, MessageService>(ServiceLifetime.Transient);
            builder.Register<IQueuedMessageProvider, MessageService>(ServiceLifetime.Transient);
        }

        // Localization
        builder.Register<ILanguagePack, LanguagePackInvariant>(ServiceLifetime.Transient);
    }
}