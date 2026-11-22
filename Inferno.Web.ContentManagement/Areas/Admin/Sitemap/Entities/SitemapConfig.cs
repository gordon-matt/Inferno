using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Entities;

/// <summary>The change frequency and priority stored for one current page version.</summary>
public class SitemapConfig : TenantEntity<int>
{
    /// <summary>Sets <see cref="Priority"/> to 0.5.</summary>
    public SitemapConfig()
    {
        Priority = .5f;
    }

    /// <summary>The identifier of the page version this row was created for, not the page's own identifier.</summary>
    public Guid PageId { get; set; }

    /// <summary>How often the URL is expected to change.</summary>
    public ChangeFrequency ChangeFrequency { get; set; }

    /// <summary>The sitemap priority, from 0 to 1. Updates are stored to one decimal place.</summary>
    public float Priority { get; set; }
}

public class SitemapConfigMap : InfernoEntityTypeConfiguration<SitemapConfig>
{
    public override void Configure(EntityTypeBuilder<SitemapConfig> builder)
    {
        builder.ToTable(CmsConstants.Tables.SitemapConfig, InfernoSchema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ChangeFrequency).IsRequired();
        builder.Property(x => x.Priority).IsRequired();
    }
}