﻿using Extenso.Data.Entity;
using Inferno.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities
{
    public class PageType : BaseEntity<Guid>
    {
        public string Name { get; set; }

        public string LayoutPath { get; set; }
    }

    public class PageTypeMap : InfernoEntityTypeConfiguration<PageType>
    {
        public override void Configure(EntityTypeBuilder<PageType> builder)
        {
            builder.ToTable(CmsConstants.Tables.PageTypes, InfernoSchema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
            builder.Property(x => x.LayoutPath).HasMaxLength(255).IsUnicode(true);
        }
    }
}