using Extenso.Data.Entity;
using Inferno.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;

/// <summary>A stored content block. The type-specific settings live in <see cref="BlockValues"/>.</summary>
public class ContentBlock : BaseEntity<Guid>
{
    /// <summary>The block type's display name.</summary>
    public string BlockName { get; set; }

    /// <summary>The block type's name, used to resolve the runtime type.</summary>
    public string BlockType { get; set; }

    /// <summary>The block's title.</summary>
    public string Title { get; set; }

    /// <summary>The zone that contains the block.</summary>
    public Guid ZoneId { get; set; }

    /// <summary>Sort position within the zone.</summary>
    public int Order { get; set; }

    /// <summary>Whether the block is shown.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>JSON values for the block type.</summary>
    public string BlockValues { get; set; }

    /// <summary>Optional path of a template that replaces the block's display component.</summary>
    public string CustomTemplatePath { get; set; }

    /// <summary>The page this block belongs to. <see langword="null"/> for a block that belongs only to the zone.</summary>
    public Guid? PageId { get; set; }
}

public class ContentBlockMap : InfernoEntityTypeConfiguration<ContentBlock>
{
    public override void Configure(EntityTypeBuilder<ContentBlock> builder)
    {
        builder.ToTable(CmsConstants.Tables.ContentBlocks, InfernoSchema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BlockName).IsRequired().HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.BlockType).IsRequired().HasMaxLength(1024).IsUnicode(false);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.ZoneId).IsRequired();
        builder.Property(x => x.Order).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.BlockValues).IsUnicode(true);
        builder.Property(x => x.CustomTemplatePath).HasMaxLength(255).IsUnicode(true);
        //builder.Property(x => x.CultureCode).HasMaxLength(10).HasColumnType("varchar");
    }
}