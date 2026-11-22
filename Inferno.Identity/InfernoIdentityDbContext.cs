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
        ScopeIdentityNamesToTenant(modelBuilder);

        var configurations = DependoResolver.Instance.ResolveAll<IInfernoEntityTypeConfiguration>();

        foreach (dynamic configuration in configurations)
        {
            modelBuilder.ApplyConfiguration(configuration);
        }
    }

    /// <summary>
    /// User names, email addresses, and role names are unique inside one tenant, and unique among host accounts.
    /// The same name can exist on two tenants. A host role is not forced to be the only role of that name.
    /// </summary>
    private static void ScopeIdentityNamesToTenant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TUser>(user =>
        {
            user.HasIndex(entry => entry.NormalizedUserName)
                .HasDatabaseName("UserNameHostIndex")
                .IsUnique()
                .HasFilter("[TenantId] IS NULL AND [NormalizedUserName] IS NOT NULL");
            user.HasIndex(entry => new { entry.TenantId, entry.NormalizedUserName })
                .HasDatabaseName("UserNameTenantIndex")
                .IsUnique()
                .HasFilter("[TenantId] IS NOT NULL AND [NormalizedUserName] IS NOT NULL");
            user.HasIndex(entry => entry.NormalizedEmail)
                .HasDatabaseName("EmailHostIndex")
                .IsUnique()
                .HasFilter("[TenantId] IS NULL AND [NormalizedEmail] IS NOT NULL");
            user.HasIndex(entry => new { entry.TenantId, entry.NormalizedEmail })
                .HasDatabaseName("EmailTenantIndex")
                .IsUnique()
                .HasFilter("[TenantId] IS NOT NULL AND [NormalizedEmail] IS NOT NULL");
        });

        modelBuilder.Entity<TRole>(role =>
        {
            role.HasIndex(entry => entry.NormalizedName)
                .HasDatabaseName("RoleNameHostIndex")
                .IsUnique()
                .HasFilter("[TenantId] IS NULL AND [NormalizedName] IS NOT NULL");
            role.HasIndex(entry => new { entry.TenantId, entry.NormalizedName })
                .HasDatabaseName("RoleNameTenantIndex")
                .IsUnique()
                .HasFilter("[TenantId] IS NOT NULL AND [NormalizedName] IS NOT NULL");
        });
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