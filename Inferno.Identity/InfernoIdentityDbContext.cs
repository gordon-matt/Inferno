﻿using Dependo;
using Inferno.Data.Entity;
using Inferno.Helpers;
using Inferno.Identity.Entities;
using Inferno.Localization;
using Inferno.Localization.Entities;
using Inferno.Tasks.Entities;
using Inferno.Tenants.Entities;
using Inferno.Web.Configuration.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LanguageEntity = Inferno.Localization.Entities.Language;

namespace Inferno.Identity
{
    public abstract class InfernoIdentityDbContext<TUser, TRole>
        : IdentityDbContext<TUser, TRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>//,
        //ISupportSeed
        where TUser : InfernoIdentityUser
        where TRole : InfernoIdentityRole
    {
        #region Constructors

        public InfernoIdentityDbContext(DbContextOptions options)
            : base(options)
        {
        }

        #endregion Constructors

        public DbSet<LanguageEntity> Languages { get; set; }

        public DbSet<LocalizableProperty> LocalizableProperties { get; set; }

        public DbSet<LocalizableString> LocalizableStrings { get; set; }

        //public DbSet<LogEntry> Log { get; set; }

        public DbSet<ScheduledTask> ScheduledTasks { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var configurations = EngineContext.Current.ResolveAll<IInfernoEntityTypeConfiguration>();

            foreach (dynamic configuration in configurations)
            {
                modelBuilder.ApplyConfiguration(configuration);
            }
        }

        #region ISupportSeed Members

        public virtual void Seed()
        {
            var tenant = new Tenant
            {
                Name = "Default",
                Url = "my-domain.com",
                Hosts = "my-domain.com"
            };

            // Create default tenant
            Tenants.Add(tenant);
            SaveChanges();

            var mediaFolder = new DirectoryInfo(CommonHelper.MapPath("~/Media/Uploads/Tenant_" + tenant.Id));
            if (!mediaFolder.Exists)
            {
                mediaFolder.Create();
            }

            InitializeLocalizableStrings();

            //var dataSettings = EngineContext.Current.Resolve<DataSettings>();

            //if (dataSettings.CreateSampleData)
            //{
            //    var seeders = EngineContext.Current.ResolveAll<IDbSeeder>().OrderBy(x => x.Order);

            //    foreach (var seeder in seeders)
            //    {
            //        seeder.Seed(this);
            //    }
            //}
        }

        #endregion ISupportSeed Members

        private void InitializeLocalizableStrings()
        {
            // We need to create localizable strings for all tenants,
            //  but at this point there will only be 1 tenant, because this is initialization for the DB.
            //  TODO: When admin user creates a new tenant, we need to insert localized strings for it. Probably in TenantApiController somewhere...
            int tenantId = Tenants.First().Id;
            var languagePacks = EngineContext.Current.ResolveAll<ILanguagePack>();

            var toInsert = new HashSet<LocalizableString>();
            foreach (var languagePack in languagePacks)
            {
                foreach (var localizedString in languagePack.LocalizedStrings)
                {
                    toInsert.Add(new LocalizableString
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        CultureCode = languagePack.CultureCode,
                        TextKey = localizedString.Key,
                        TextValue = localizedString.Value
                    });
                }
            }
            LocalizableStrings.AddRange(toInsert);
            SaveChanges();
        }
    }
}