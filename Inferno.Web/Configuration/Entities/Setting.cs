using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.Configuration.Entities
{
    public class Setting : TenantEntity<Guid>
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public override string ToString() => Name;
    }

    public class SettingMap : IEntityTypeConfiguration<Setting>, IInfernoEntityTypeConfiguration
    {
        public void Configure(EntityTypeBuilder<Setting> builder)
        {
            builder.ToTable("Inferno_Settings");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
            builder.Property(s => s.Type).IsRequired().HasMaxLength(255).IsUnicode(false);
            builder.Property(s => s.Value).IsUnicode(true);
        }

        #region IEntityTypeConfiguration Members

        public bool IsEnabled => true;

        #endregion IEntityTypeConfiguration Members
    }
}