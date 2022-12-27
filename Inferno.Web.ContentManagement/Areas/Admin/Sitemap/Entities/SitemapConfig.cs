using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Entities
{
    public class SitemapConfig : TenantEntity<int>
    {
        public SitemapConfig()
        {
            Priority = .5f;
        }

        public Guid PageId { get; set; }

        public ChangeFrequency ChangeFrequency { get; set; }

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
}