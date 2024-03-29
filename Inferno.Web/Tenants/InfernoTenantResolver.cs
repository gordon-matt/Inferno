﻿using Dependo;
using Extenso.Collections;
using Inferno.Tenants;
using Inferno.Tenants.Entities;
using Inferno.Tenants.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SaasKit.Multitenancy;

namespace Inferno.Web.Tenants
{
    public class InfernoTenantResolver : MemoryCacheTenantResolver<Tenant>
    {
        private readonly ITenantService tenantService;
        private IEnumerable<Tenant> tenants;

        public InfernoTenantResolver(
            IMemoryCache cache,
            ILoggerFactory loggerFactory,
            ITenantService tenantService)
            : base(cache, loggerFactory)
        {
            this.tenantService = tenantService;
            tenants = tenantService.Find();
        }

        protected override string GetContextIdentifier(HttpContext context)
        {
            return context.Request.Host.Value.ToLower();
        }

        protected override IEnumerable<string> GetTenantIdentifiers(TenantContext<Tenant> context)
        {
            return context.Tenant.Hosts.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Wrong Usage", "DF0010:Marks undisposed local variables.", Justification = "TenantContext is handled by SaasKit")]
        protected override Task<TenantContext<Tenant>> ResolveAsync(HttpContext context)
        {
            TenantContext<Tenant> tenantContext = null;

            var loggerFactory = EngineContext.Current.Resolve<ILoggerFactory>();
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

                if (tenants.IsNullOrEmpty())
                {
                    tenants = tenantService.Find();
                }

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

                tenant ??= tenants.First();

                logger.LogInformation("[Tenant]: ID: {TenantID}, Name: {TenantName}, Hosts: {TenantHosts}", tenant.Id, tenant.Name, tenant.Hosts);
                tenantContext = new TenantContext<Tenant>(tenant);
            }
            catch (Exception x)
            {
                logger.LogError(new EventId(), x, x.GetBaseException().Message);
            }

            return Task.FromResult(tenantContext);
        }

        protected override MemoryCacheEntryOptions CreateCacheEntryOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(new TimeSpan(24, 0, 0)); // Cache for 24 hours
        }
    }
}