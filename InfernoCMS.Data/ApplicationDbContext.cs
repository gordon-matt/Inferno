using Inferno.Identity;
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
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new PersonMap());
        }
    }
}