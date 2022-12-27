using Inferno.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Localization.Entities
{
    public class LocalizableStringMap : InfernoEntityTypeConfiguration<LocalizableString>
    {
        public override void Configure(EntityTypeBuilder<LocalizableString> builder)
        {
            builder.ToTable("LocalizableStrings", InfernoSchema);
            builder.HasKey(m => m.Id);
            builder.Property(m => m.CultureCode).HasMaxLength(10).IsUnicode(false);
            builder.Property(m => m.TextKey).IsRequired().IsUnicode(true);
            builder.Property(m => m.TextValue).IsUnicode(true);
        }
    }
}