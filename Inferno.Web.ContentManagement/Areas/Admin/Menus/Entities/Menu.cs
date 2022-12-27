using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities
{
    public class Menu : TenantEntity<Guid>
    {
        public string Name { get; set; }

        public string UrlFilter { get; set; }
    }

    public class MenuMap : InfernoEntityTypeConfiguration<Menu>
    {
        public override void Configure(EntityTypeBuilder<Menu> builder)
        {
            builder.ToTable(CmsConstants.Tables.Menus, InfernoSchema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
            builder.Property(x => x.UrlFilter).HasMaxLength(255).IsUnicode(true);
        }
    }
}