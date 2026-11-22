using Inferno.Identity;
using InfernoCMS.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace InfernoCMS.Data;

/// <summary>The Inferno CMS database, including Identity users and profile rows.</summary>
public class ApplicationDbContext : InfernoIdentityDbContext<ApplicationUser, ApplicationRole>
{
    /// <summary>Creates a context with <paramref name="options"/>.</summary>
    /// <param name="options">The Entity Framework options for this context.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>Profile rows stored for application users.</summary>
    public DbSet<UserProfileEntry> UserProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().HasMany(x => x.Roles).WithOne().HasForeignKey(x => x.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ApplicationUser>().HasMany(x => x.Claims).WithOne().HasForeignKey(x => x.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ApplicationRole>().HasMany(x => x.Claims).WithOne().HasForeignKey(x => x.RoleId).IsRequired().OnDelete(DeleteBehavior.Cascade);
    }
}