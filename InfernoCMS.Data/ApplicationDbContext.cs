using Inferno.Identity;
using Inferno.Localization.Entities;
using Inferno.Tasks.Entities;
using Inferno.Tenants.Entities;
using Inferno.Web.Configuration.Entities;
using InfernoCMS.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace InfernoCMS.Data
{
    public class ApplicationDbContext : InfernoIdentityDbContext<ApplicationUser, ApplicationRole>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Person> People { get; set; } // Sample Data (TODO: Remove)

        public DbSet<UserProfileEntry> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //base.OnModelCreating(builder);
            builder.ApplyConfiguration(new PersonMap());

            // Design-time only
            BaseOnModelCreating(builder);
            builder.ApplyConfiguration(new LanguageMap());
            builder.ApplyConfiguration(new LocalizablePropertyMap());
            builder.ApplyConfiguration(new LocalizableStringMap());
            builder.ApplyConfiguration(new ScheduledTaskMap());
            builder.ApplyConfiguration(new TenantMap());
            builder.ApplyConfiguration(new SettingMap());
            builder.ApplyConfiguration(new UserProfileEntryMap());
            // END: Design-time only
        }
    }
}