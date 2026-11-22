using Dependo;
using Extenso.Collections;
using Extenso.Data.Entity;
using Flatmate;
using Inferno.Tenants;
using Inferno.Tenants.Entities;
using Inferno.Tenants.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Tenants;

/// <summary>
/// Resolves a tenant from the request host and caches it for 24 hours.
/// An unlisted host matches only when the site has a single tenant. A site with no tenants gets a default one for that host.
/// </summary>
public class InfernoTenantResolver : MemoryCacheTenantResolver<Tenant>
{
    private readonly ITenantService tenantService;

    /// <summary>Creates the resolver.</summary>
    /// <param name="cache">The memory cache the base class stores tenants in.</param>
    /// <param name="loggerFactory">Passed to the base resolver.</param>
    /// <param name="tenantService">Reads and, when none exist, inserts tenants.</param>
    public InfernoTenantResolver(
        IMemoryCache cache,
        ILoggerFactory loggerFactory,
        ITenantService tenantService)
        : base(cache, loggerFactory)
    {
        this.tenantService = tenantService;
    }

    /// <summary>The request host, lowercased.</summary>
    /// <param name="context">The current request.</param>
    /// <returns>The host, including the port.</returns>
    protected override string GetContextIdentifier(HttpContext context) => context.Request.Host.Value.ToLower();

    /// <summary>The hosts stored on <paramref name="context"/>, split on commas.</summary>
    /// <param name="context">The resolved tenant.</param>
    /// <returns>The host identifiers, with empty entries removed.</returns>
    protected override IEnumerable<string> GetTenantIdentifiers(TenantContext<Tenant> context) => context.Tenant.Hosts.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// Loads every tenant and matches <paramref name="context"/>'s host. An empty host is matched as <c>unknown-host</c>.
    /// A lookup that throws is logged and returns <see langword="null"/>.
    /// </summary>
    /// <param name="context">The current request.</param>
    /// <param name="cancellationToken">Not observed.</param>
    /// <returns>The tenant context, or <see langword="null"/> when no tenant matches.</returns>
    protected override Task<TenantContext<Tenant>> ResolveFromStoreAsync(HttpContext context, CancellationToken cancellationToken)
    {
        TenantContext<Tenant> tenantContext = null;

        var loggerFactory = DependoResolver.Instance.Resolve<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(this.GetType());

        try
        {
            string host = context.Request.Host.Value.ToLower();

            if (string.IsNullOrEmpty(host))
            {
                host = "unknown-host";
            }

            logger.LogInformation("[Host]: {Host}", host);

            Tenant tenant;

            // Read on every lookup that misses the cache, so tenants added since startup are found.
            var tenants = tenantService.Find(new SearchOptions<Tenant>
            {
                Query = x => true
            });

            if (tenants.IsNullOrEmpty())
            {
                logger.LogError("No tenants found! Inserting a new one...");
                tenant = new Tenant
                {
                    Name = "Default Tenant",
                    Hosts = host,
                    Url = host
                };
                tenantService.Insert(tenant);
            }
            else
            {
                tenant = tenants.FirstOrDefault(s => s.ContainsHostValue(host));
            }

            // An unlisted host belongs to nobody, unless the site only has the one tenant. Guessing the first
            // would show one tenant's site, and its sign-in, under a host name that was never given to it.
            tenant ??= tenants.Count() == 1 ? tenants.First() : null;

            if (tenant is null)
            {
                logger.LogWarning("[Tenant]: No tenant has the host {Host}.", host);
            }
            else
            {
                logger.LogInformation("[Tenant]: ID: {TenantID}, Name: {TenantName}, Hosts: {TenantHosts}", tenant.Id, tenant.Name, tenant.Hosts);
                tenantContext = new TenantContext<Tenant>(tenant);
            }
        }
        catch (Exception x)
        {
            logger.LogError(new EventId(), x, x.GetBaseException().Message);
        }

        return Task.FromResult(tenantContext);
    }

    /// <summary>Caches a resolved tenant for 24 hours.</summary>
    /// <returns>Entry options with a 24-hour absolute expiration.</returns>
    protected override MemoryCacheEntryOptions CreateCacheEntryOptions() => new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(new TimeSpan(24, 0, 0)); // Cache for 24 hours
}