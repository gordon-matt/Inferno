using System.Runtime.Serialization;
using Extenso.Data.Entity;
using Inferno.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Entities
{
    public class BlogPostTag : IEntity
    {
        public Guid PostId { get; set; }

        public int TagId { get; set; }

        public BlogPost Post { get; set; }

        public BlogTag Tag { get; set; }

        #region IEntity Members

        [IgnoreDataMember]
        public object[] KeyValues => new object[] { PostId, TagId };

        #endregion IEntity Members
    }

    public class PostTagMap : InfernoEntityTypeConfiguration<BlogPostTag>
    {
        public override void Configure(EntityTypeBuilder<BlogPostTag> builder)
        {
            builder.ToTable(CmsConstants.Tables.BlogPostTags, InfernoSchema);
            builder.HasKey(x => new { x.PostId, x.TagId });

            builder.HasOne(x => x.Post).WithMany(x => x.Tags).HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Tag).WithMany(x => x.Posts).HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.TagId);
        }
    }
}