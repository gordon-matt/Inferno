using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Entities;

public class BlogTag : TenantEntity<int>
{
    public string Name { get; set; }

    public string UrlSlug { get; set; }

    public ICollection<BlogPostTag> Posts
    {
        get => field ??= new HashSet<BlogPostTag>();
        set;
    }
}

public class TagMap : InfernoEntityTypeConfiguration<BlogTag>
{
    public override void Configure(EntityTypeBuilder<BlogTag> builder)
    {
        builder.ToTable(CmsConstants.Tables.BlogTags, InfernoSchema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.UrlSlug).IsRequired().HasMaxLength(255).IsUnicode(true);
    }
}