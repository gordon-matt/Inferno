using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Inferno.Web.Configuration.Services;
using Module = Autofac.Module;

namespace Inferno.Web.Configuration;

/// <summary>Registers every concrete <see cref="ISettings"/> so resolving it loads the current tenant's stored copy.</summary>
public class ConfigurationModule : Module
{
    /// <summary>Adds <see cref="SettingsSource"/> to the container.</summary>
    /// <param name="builder">The Autofac builder.</param>
    protected override void Load(ContainerBuilder builder) => builder.RegisterSource(new SettingsSource());

    /// <summary>Creates a per-scope registration for each concrete <see cref="ISettings"/> type that is requested.</summary>
    public class SettingsSource : IRegistrationSource
    {
        private static readonly MethodInfo buildMethod = typeof(SettingsSource).GetMethod("BuildRegistration",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>Yields one registration when <paramref name="service"/> is a concrete <see cref="ISettings"/> class.</summary>
        /// <param name="service">The service being resolved.</param>
        /// <param name="registrationAccessor">Unused. Required by <see cref="IRegistrationSource"/>.</param>
        /// <returns>The settings registration, or none.</returns>
        public IEnumerable<IComponentRegistration> RegistrationsFor(
            Service service,
            Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            var ts = service as TypedService;
            if (ts != null)
            {
                var typeInfo = ts.ServiceType.GetTypeInfo();
                if (typeInfo.IsClass && !typeInfo.IsAbstract && typeof(ISettings).IsAssignableFrom(ts.ServiceType))
                {
                    var buildGenericMethod = buildMethod.MakeGenericMethod(ts.ServiceType);
                    yield return (IComponentRegistration)buildGenericMethod.Invoke(null, null);
                }
            }
        }

        internal static IComponentRegistration BuildRegistration<TSettings>() where TSettings : ISettings, new() => RegistrationBuilder
                .ForDelegate((c, p) =>
                {
                    int currentTenantId = c.Resolve<IWorkContext>().CurrentTenant.Id;
                    // Uncomment the code below if you want load settings per tenant only when you have two tenants installed.
                    //var currentTenantId = c.Resolve<IWorkContext>().GetAllTenants().Count > 1
                    //    c.Resolve<IWorkContext>().CurrentTenant.Id : 0;

                    // Although it's better to connect to your database and execute the following SQL:
                    //DELETE FROM [Setting] WHERE [TenantId] > 0
                    return c.Resolve<ISettingService>().GetSettings<TSettings>(currentTenantId);
                })
                .InstancePerLifetimeScope()
                .CreateRegistration();

        /// <summary>Always <see langword="false"/>. This source is not an adapter for one component.</summary>
        public bool IsAdapterForIndividualComponents => false;
    }
}