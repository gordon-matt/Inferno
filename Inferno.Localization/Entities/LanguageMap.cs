﻿using Inferno.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Localization.Entities
{
    public class LanguageMap : IEntityTypeConfiguration<Language>, IInfernoEntityTypeConfiguration
    {
        public void Configure(EntityTypeBuilder<Language> builder)
        {
            builder.ToTable("Languages", "inferno");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
            builder.Property(m => m.CultureCode).IsRequired().HasMaxLength(10).IsUnicode(false);
            builder.Property(m => m.IsRTL).IsRequired();
            builder.Property(m => m.IsEnabled).IsRequired();
            builder.Property(m => m.SortOrder).IsRequired();
        }

        #region IEntityTypeConfiguration Members

        public bool IsEnabled
        {
            get { return true; }
        }

        #endregion IEntityTypeConfiguration Members
    }
}