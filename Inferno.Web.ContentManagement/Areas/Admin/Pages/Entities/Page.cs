﻿using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities
{
    public class Page : TenantEntity<Guid>
    {
        private ICollection<PageVersion> versions;

        public Guid? ParentId { get; set; }

        public Guid PageTypeId { get; set; }

        public string Name { get; set; }

        public bool IsEnabled { get; set; }

        public int Order { get; set; }

        public bool ShowOnMenus { get; set; }

        public string AccessRestrictions { get; set; }

        public ICollection<PageVersion> Versions
        {
            get { return versions ??= new HashSet<PageVersion>(); }
            set { versions = value; }
        }
    }

    public class PageMap : InfernoEntityTypeConfiguration<Page>
    {
        public override void Configure(EntityTypeBuilder<Page> builder)
        {
            builder.ToTable(CmsConstants.Tables.Pages, InfernoSchema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PageTypeId).IsRequired();
            builder.Property(x => x.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
            builder.Property(x => x.IsEnabled).IsRequired();
            builder.Property(x => x.Order).IsRequired();
            builder.Property(x => x.ShowOnMenus).IsRequired();
            builder.Property(x => x.AccessRestrictions).HasMaxLength(1024).IsUnicode(false);
        }
    }
}