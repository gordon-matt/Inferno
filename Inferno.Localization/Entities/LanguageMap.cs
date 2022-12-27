using Inferno.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Localization.Entities
{
    public class LanguageMap : InfernoEntityTypeConfiguration<Language>
    {
        public override void Configure(EntityTypeBuilder<Language> builder)
        {
            builder.ToTable("Languages", InfernoSchema);
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
            builder.Property(m => m.CultureCode).IsRequired().HasMaxLength(10).IsUnicode(false);
            builder.Property(m => m.IsRTL).IsRequired();
            builder.Property(m => m.IsEnabled).IsRequired();
            builder.Property(m => m.SortOrder).IsRequired();
        }
    }
}