using Dependo;
using Inferno.Data.Entity;
using Inferno.Helpers;
using Inferno.Identity.Entities;
using Inferno.Tenants.Entities;
using Inferno.Web.Configuration.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace Inferno.Identity;

/// <summary>Identity database context that also stores tenants and settings, and applies Inferno entity configurations.</summary>
/// <typeparam name="TUser">The user type.</typeparam>
/// <typeparam name="TRole">The role type.</typeparam>
public abstract class InfernoIdentityDbContext<TUser, TRole>
    : IdentityDbContext<TUser, TRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    where TUser : InfernoIdentityUser
    where TRole : InfernoIdentityRole
{
    #region Constructors

    /// <summary>Creates a context with <paramref name="options"/>.</summary>
    /// <param name="options">The Entity Framework options.</param>
    public InfernoIdentityDbContext(DbContextOptions options)
        : base(options)
    {
    }

    #endregion Constructors

    /// <summary>Application settings rows.</summary>
    public DbSet<Setting> Settings { get; set; }

    /// <summary>Tenants stored in this database.</summary>
    public DbSet<Tenant> Tenants { get; set; }

    /// <summary>Runs the Identity model configuration from the base context.</summary>
    /// <param name="modelBuilder">The model builder.</param>
    public void BaseOnModelCreating(ModelBuilder modelBuilder) => base.OnModelCreating(modelBuilder);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var configurations = DependoResolver.Instance.ResolveAll<IInfernoEntityTypeConfiguration>();

        foreach (dynamic configuration in configurations)
        {
            modelBuilder.ApplyConfiguration(configuration);
        }
    }

    /// <summary>Inserts a tenant named Default at my-domain.com and creates its media upload folder when that folder is missing.</summary>
    public virtual void Seed()
    {
        var tenant = new Tenant
        {
            Name = "Default",
            Url = "my-domain.com",
            Hosts = "my-domain.com"
        };

        Tenants.Add(tenant);
        SaveChanges();

        var mediaFolder = new DirectoryInfo(CommonHelper.MapPath("~/Media/Uploads/Tenant_" + tenant.Id));
        if (!mediaFolder.Exists)
        {
            mediaFolder.Create();
        }
    }

}