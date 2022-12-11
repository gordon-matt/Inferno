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

        public DbSet<Permission> Permissions { get; set; }

        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<UserProfileEntry> UserProfiles { get; set; }

        public DbSet<Person> People { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new PersonMap());
        }
    }
}