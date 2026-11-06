using System.Collections.Concurrent;
using Dependo;
using Extenso;
using Extenso.Data.Entity;
using Flatmate;
using Inferno.Exceptions;
using Inferno.Security.Membership;
using Inferno.Tenants;
using Inferno.Tenants.Entities;
using Inferno.Tenants.Services;
using Inferno.Web.Navigation;

namespace Inferno.Web;

public partial class WorkContext : IWorkContext
{
    private Tenant cachedTenant;

    private readonly IWebHelper webHelper;
    private readonly ConcurrentDictionary<string, Func<object>> stateResolvers = new();
    private readonly IEnumerable<IWorkContextStateProvider> workContextStateProviders;

    public WorkContext()
    {
        webHelper = DependoResolver.Instance.Resolve<IWebHelper>();
        workContextStateProviders = DependoResolver.Instance.ResolveAll<IWorkContextStateProvider>();
        Breadcrumbs = [];
    }

    #region IWorkContext Members

    public T GetState<T>(string name)
    {
        var resolver = stateResolvers.GetOrAdd(name, FindResolverForState<T>);
        return (T)resolver();
    }

    public void SetState<T>(string name, T value) => stateResolvers[name] = () => value;

    public BreadcrumbCollection Breadcrumbs { get; set; }

    public string CurrentCultureCode => GetState<string>(InfernoWebConstants.StateProviders.CurrentCultureCode);

    public InfernoUser CurrentUser => GetState<InfernoUser>(InfernoWebConstants.StateProviders.CurrentUser);

    public virtual Tenant CurrentTenant
    {
        get
        {
            if (cachedTenant != null)
            {
                return cachedTenant;
            }

            // The tenant Flatmate resolved for this request is the one the theme, blog and identity
            // code already use, so take it. Resolving the host a second way let the two disagree.
            try
            {
                if (DependoResolver.Instance.Resolve<ICurrentTenant<Tenant>>()?.Tenant is { } resolved)
                {
                    cachedTenant = resolved;
                    return cachedTenant;
                }
            }
            catch (Exception)
            {
                // Outside a request there is nothing resolved, so fall through to the host.
            }

            try
            {
                // No resolved tenant (a background job, say). Match by HTTP_HOST, with the port first
                // because tenants list their hosts that way, then without it.
                // GetUrlHost is a full URL. Tenants store the host header, such as localhost:5001.
                string host = webHelper.GetUrlHost();
                if (Uri.TryCreate(host, UriKind.Absolute, out Uri absolute))
                {
                    host = absolute.Authority;
                }

                var tenantService = DependoResolver.Instance.Resolve<ITenantService>();
                var allTenants = tenantService.Find(new SearchOptions<Tenant>
                {
                    Query = x => true
                });
                var tenant = allTenants.FirstOrDefault(s => s.ContainsHostValue(host))
                    ?? (host.Contains(':') ? allTenants.FirstOrDefault(s => s.ContainsHostValue(host.LeftOf(':'))) : null);

                // An unlisted host belongs to nobody, unless the site only has the one tenant.
                tenant ??= allTenants.Count() == 1 ? allTenants.First() : null;
                cachedTenant = tenant ?? throw new InfernoException("No tenant could be loaded");
                return cachedTenant;
            }
            catch
            {
                return null;
            }
        }
    }

    public IDisposable ActAsTenant(Tenant tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        var previous = cachedTenant;
        cachedTenant = tenant;
        return new TenantScope(() => cachedTenant = previous);
    }

    #endregion IWorkContext Members

    private sealed class TenantScope(Action restore) : IDisposable
    {
        public void Dispose() => restore();
    }

    private Func<object> FindResolverForState<T>(string name)
    {
        var resolver = workContextStateProviders
            .Select(wcsp => wcsp.Get<T>(name))
            .FirstOrDefault(value => !Equals(value, default(T)));

        return resolver == null ? (() => default(T)) : (() => resolver(this));
    }
}