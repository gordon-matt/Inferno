using Dependo;
using Inferno.Data.Entity;
using Inferno.Helpers;
using Inferno.Identity.Entities;
using Inferno.Tasks.Entities;
using Inferno.Tenants.Entities;
using Inferno.Web.Configuration.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace Inferno.Identity;

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

    public DbSet<ScheduledTask> ScheduledTasks { get; set; }

    public DbSet<Setting> Settings { get; set; }

    public DbSet<Tenant> Tenants { get; set; }

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

        //var dataSettings = DependoResolver.Instance.Resolve<DataSettings>();

        //if (dataSettings.CreateSampleData)
        //{
        //    var seeders = DependoResolver.Instance.ResolveAll<IDbSeeder>().OrderBy(x => x.Order);

        //    foreach (var seeder in seeders)
        //    {
        //        seeder.Seed(this);
        //    }
        //}
    }

    #endregion ISupportSeed Members

}