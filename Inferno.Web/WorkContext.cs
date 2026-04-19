using System.Collections.Concurrent;
using Dependo;
using Extenso;
using Extenso.Data.Entity;
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

    public string CurrentTheme
    {
        get => GetState<string>(InfernoWebConstants.StateProviders.CurrentTheme);
        set => SetState(InfernoWebConstants.StateProviders.CurrentTheme, value);
    }

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

            try
            {
                // Try to determine the current tenant by HTTP_HOST
                string host = webHelper.GetUrlHost();

                if (host.Contains(':'))
                {
                    host = host.LeftOf(':');
                }

                var tenantService = DependoResolver.Instance.Resolve<ITenantService>();
                var allTenants = tenantService.Find(new SearchOptions<Tenant>
                {
                    Query = x => true
                });
                var tenant = allTenants.FirstOrDefault(s => s.ContainsHostValue(host));

                // Load the first found tenant
                tenant ??= allTenants.FirstOrDefault();
                cachedTenant = tenant ?? throw new InfernoException("No tenant could be loaded");
                return cachedTenant;
            }
            catch
            {
                return null;
            }
        }
    }

    #endregion IWorkContext Members

    private Func<object> FindResolverForState<T>(string name)
    {
        var resolver = workContextStateProviders
            .Select(wcsp => wcsp.Get<T>(name))
            .FirstOrDefault(value => !Equals(value, default(T)));

        return resolver == null ? (() => default(T)) : (() => resolver(this));
    }
}