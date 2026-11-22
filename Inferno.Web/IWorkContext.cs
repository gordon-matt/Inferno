using Inferno.Security.Membership;
using Inferno.Tenants.Entities;
using Inferno.Web.Navigation;

namespace Inferno.Web;

/// <summary>The tenant, user, culture, and other state for the current operation.</summary>
public interface IWorkContext
{
    /// <summary>
    /// Returns the value stored under <paramref name="name"/>. The first <see cref="IWorkContextStateProvider"/>
    /// that supplies it is remembered for later calls.
    /// </summary>
    /// <typeparam name="T">The type of the stored value.</typeparam>
    /// <param name="name">The state name.</param>
    /// <returns>The stored value, or the default for <typeparamref name="T"/> when nothing supplies it.</returns>
    T GetState<T>(string name);

    /// <summary>Stores <paramref name="value"/> under <paramref name="name"/>, replacing any provider for that name.</summary>
    /// <typeparam name="T">The type of the stored value.</typeparam>
    /// <param name="name">The state name.</param>
    /// <param name="value">The value later calls to <see cref="GetState{T}"/> return.</param>
    void SetState<T>(string name, T value);

    /// <summary>The culture code from <see cref="InfernoWebConstants.StateProviders.CurrentCultureCode"/>.</summary>
    string CurrentCultureCode { get; }

    /// <summary>
    /// The tenant for this operation. A tenant already resolved for the request is used. Otherwise the host is matched
    /// against the tenants' host lists, and a site with only one tenant uses that tenant. An unlisted host on a
    /// multi-tenant site, or a failed lookup, yields <see langword="null"/>.
    /// </summary>
    Tenant CurrentTenant { get; }

    /// <summary>
    /// Makes <paramref name="tenant"/> the current tenant until the result is disposed. For code that runs outside a
    /// request yet acts for one tenant, such as seeding data or a background job. Identity's role and user lookups
    /// follow the current tenant, so they only see that tenant's records while it is in effect.
    /// </summary>
    /// <param name="tenant">The tenant to act as.</param>
    /// <returns>A scope that restores the previous tenant when disposed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tenant"/> is <see langword="null"/>.</exception>
    IDisposable ActAsTenant(Tenant tenant);

    /// <summary>The user from <see cref="InfernoWebConstants.StateProviders.CurrentUser"/>.</summary>
    InfernoUser CurrentUser { get; }

    /// <summary>The breadcrumb trail. Empty until a caller sets it.</summary>
    BreadcrumbCollection Breadcrumbs { get; set; }
}