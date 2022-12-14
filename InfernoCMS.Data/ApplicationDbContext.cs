using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Extensions;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.EntityFramework.Options;
using Inferno.Identity;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace InfernoCMS.Data
{
    public class ApplicationDbContext : InfernoIdentityDbContext<ApplicationUser, ApplicationRole>, IConfigurationDbContext, IPersistedGrantDbContext
    {
        public ConfigurationStoreOptions ConfigurationStoreOptions { get; set; }

        public OperationalStoreOptions OperationalStoreOptions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Permission> Permissions { get; set; }

        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<UserProfileEntry> UserProfiles { get; set; }

        public DbSet<Person> People { get; set; }

        #region IConfigurationDbContext Members

        public DbSet<Client> Clients { get; set; }

        public DbSet<ClientCorsOrigin> ClientCorsOrigins { get; set; }

        public DbSet<IdentityResource> IdentityResources { get; set; }

        public DbSet<ApiResource> ApiResources { get; set; }

        public DbSet<ApiScope> ApiScopes { get; set; }

        public DbSet<IdentityProvider> IdentityProviders { get; set; }

        #endregion IConfigurationDbContext Members

        #region IPersistedGrantDbContext Members

        public DbSet<PersistedGrant> PersistedGrants { get; set; }

        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }

        public DbSet<Key> Keys { get; set; }

        public DbSet<ServerSideSession> ServerSideSessions { get; set; }

        #endregion IPersistedGrantDbContext Members

        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (ConfigurationStoreOptions is null)
            {
                ConfigurationStoreOptions = this.GetService<ConfigurationStoreOptions>();
                if (ConfigurationStoreOptions is null)
                {
                    //ConfigurationStoreOptions = new ConfigurationStoreOptions();
                    throw new ArgumentNullException(nameof(ConfigurationStoreOptions), "ConfigurationStoreOptions must be configured in the DI system.");
                }
            }
            builder.ConfigureClientContext(ConfigurationStoreOptions);

            if (OperationalStoreOptions is null)
            {
                OperationalStoreOptions = this.GetService<OperationalStoreOptions>();
                if (OperationalStoreOptions is null)
                {
                    //OperationalStoreOptions = new OperationalStoreOptions();
                    throw new ArgumentNullException(nameof(OperationalStoreOptions), "OperationalStoreOptions must be configured in the DI system.");
                }
            }
            builder.ConfigurePersistedGrantContext(OperationalStoreOptions);

            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new PersonMap());
        }
    }
}