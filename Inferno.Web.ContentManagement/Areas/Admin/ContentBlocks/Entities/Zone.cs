using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;

/// <summary>A named region of a tenant's layout that holds content blocks.</summary>
public class Zone : TenantEntity<Guid>
{
    /// <summary>The zone name a layout asks for.</summary>
    public string Name { get; set; }
}

public class ZoneMap : InfernoEntityTypeConfiguration<Zone>
{
    public override void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.ToTable(CmsConstants.Tables.Zones, InfernoSchema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
    }
}