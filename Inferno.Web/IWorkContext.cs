using Inferno.Security.Membership;
using Inferno.Tenants.Entities;
using Inferno.Web.Navigation;

namespace Inferno.Web;

public interface IWorkContext
{
    T GetState<T>(string name);

    void SetState<T>(string name, T value);

    string CurrentCultureCode { get; }

    Tenant CurrentTenant { get; }

    /// <summary>
    /// Makes <paramref name="tenant"/> the current tenant until the result is disposed. For code that runs outside a
    /// request yet acts for one tenant, such as seeding data or a background job. Identity's role and user lookups
    /// follow the current tenant, so they only see that tenant's records while it is in effect.
    /// </summary>
    IDisposable ActAsTenant(Tenant tenant);

    InfernoUser CurrentUser { get; }

    BreadcrumbCollection Breadcrumbs { get; set; }
}