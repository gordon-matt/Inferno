using Flatmate;
using Inferno.Identity;
using Xunit;
using Inferno.Security.Membership;
using Inferno.Tenants.Entities;
using Inferno.Web;
using Inferno.Web.Navigation;

namespace Inferno.Tests;

public class CurrentTenantIdTests
{
    [Fact]
    public void Resolve_NoTenantInScope_ReturnsNull()
    {
        var services = new ServiceProvider()
            .With<ICurrentTenant<Tenant>>(new FixedTenant(null))
            .With<IWorkContext>(new FixedWorkContext(null));

        Assert.Null(CurrentTenantId.Resolve(services));
    }

    [Fact]
    public void Resolve_FlatmateTenant_ReturnsItsId()
    {
        var services = new ServiceProvider().With<ICurrentTenant<Tenant>>(new FixedTenant(Tenant(7)));

        Assert.Equal(7, CurrentTenantId.Resolve(services));
    }

    [Fact]
    public void Resolve_NoFlatmateTenant_UsesTheWorkContext()
    {
        var services = new ServiceProvider()
            .With<ICurrentTenant<Tenant>>(new FixedTenant(null))
            .With<IWorkContext>(new FixedWorkContext(Tenant(4)));

        Assert.Equal(4, CurrentTenantId.Resolve(services));
    }

    [Fact]
    public void Resolve_AmbientScope_UsesThatScope()
    {
        var services = new ServiceProvider().With<ICurrentTenant<Tenant>>(new FixedTenant(Tenant(9)));

        using (CurrentTenantId.Use(services))
        {
            Assert.Equal(9, CurrentTenantId.Resolve());
        }
    }

    private static Tenant Tenant(int id) => new() { Id = id, Name = "Tenant " + id };

    private sealed class FixedTenant(Tenant tenant) : ICurrentTenant<Tenant>
    {
        public TenantContext<Tenant> Context => null;

        public Tenant Tenant => tenant;
    }

    private sealed class FixedWorkContext(Tenant tenant) : IWorkContext
    {
        public Tenant CurrentTenant => tenant;

        public string CurrentCultureCode => null;

        public InfernoUser CurrentUser => null;

        public BreadcrumbCollection Breadcrumbs { get; set; }

        public IDisposable ActAsTenant(Tenant tenant) => throw new NotSupportedException();

        public T GetState<T>(string name) => throw new NotSupportedException();

        public void SetState<T>(string name, T value) => throw new NotSupportedException();
    }

    private sealed class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> services = [];

        public ServiceProvider With<T>(T service)
            where T : class
        {
            services[typeof(T)] = service;
            return this;
        }

        public object GetService(Type serviceType) => services.TryGetValue(serviceType, out var service) ? service : null;
    }
}
