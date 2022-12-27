using Inferno.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Tenants.Entities
{
    public class TenantMap : InfernoEntityTypeConfiguration<Tenant>
    {
        public override void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.ToTable("Tenants", InfernoSchema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
            builder.Property(x => x.Url).IsRequired().HasMaxLength(255).IsUnicode(true);
            //builder.Property(x => x.SecureUrl).HasMaxLength(255).IsUnicode(true);
            builder.Property(x => x.Hosts).HasMaxLength(1024).IsUnicode(true);
        }
    }
}