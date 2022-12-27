﻿using Extenso.Data.Entity;
using Inferno.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities
{
    public class MenuItem : BaseEntity<Guid>
    {
        public Guid MenuId { get; set; }

        public string Text { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public string CssClass { get; set; }

        public int Position { get; set; }

        public Guid? ParentId { get; set; }

        public bool Enabled { get; set; }

        public bool IsExternalUrl { get; set; }

        public Guid? RefId { get; set; }

        //public virtual Menu Menu { get; set; }

        //public virtual MenuItem Parent { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public class MenuItemMap : InfernoEntityTypeConfiguration<MenuItem>
    {
        public override void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            builder.ToTable(CmsConstants.Tables.MenuItems, InfernoSchema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.MenuId).IsRequired();
            builder.Property(x => x.Text).IsRequired().HasMaxLength(255).IsUnicode(true);
            builder.Property(x => x.Description).HasMaxLength(255).IsUnicode(true);
            builder.Property(x => x.Url).IsRequired().HasMaxLength(255).IsUnicode(true);
            builder.Property(x => x.CssClass).HasMaxLength(128).IsUnicode(false);
            builder.Property(x => x.Position).IsRequired();
            builder.Property(x => x.Enabled).IsRequired();
            builder.Property(x => x.IsExternalUrl).IsRequired();

            //HasRequired(x => x.Menu).WithMany().HasForeignKey(x => x.MenuId);
            //HasOptional(x => x.Parent).WithMany().HasForeignKey(x => x.ParentId);
        }
    }
}