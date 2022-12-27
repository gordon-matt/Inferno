using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Entities
{
    public class BlogCategory : TenantEntity<int>
    {
        private ICollection<BlogPost> posts;

        public string Name { get; set; }

        public string UrlSlug { get; set; }

        public ICollection<BlogPost> Posts
        {
            get { return posts ??= new HashSet<BlogPost>(); }
            set { posts = value; }
        }
    }

    public class CategoryMap : InfernoEntityTypeConfiguration<BlogCategory>
    {
        public override void Configure(EntityTypeBuilder<BlogCategory> builder)
        {
            builder.ToTable(CmsConstants.Tables.BlogCategories, InfernoSchema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
            builder.Property(x => x.UrlSlug).IsRequired().HasMaxLength(255).IsUnicode(true);
        }
    }
}