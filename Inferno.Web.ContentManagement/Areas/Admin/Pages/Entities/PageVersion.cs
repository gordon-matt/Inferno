using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;

/// <summary>One saved copy of a page, for a culture and a status.</summary>
public class PageVersion : TenantEntity<Guid>
{
    /// <summary>The page this version belongs to.</summary>
    public Guid PageId { get; set; }

    /// <summary>The culture of this version. <see langword="null"/> for the invariant version.</summary>
    public string CultureCode { get; set; }

    /// <summary>When the version was first saved, in UTC.</summary>
    public DateTime DateCreatedUtc { get; set; }

    /// <summary>When the version was last saved, in UTC. Newer values win when several versions match.</summary>
    public DateTime DateModifiedUtc { get; set; }

    /// <summary>Whether this version is a draft, published, or archived.</summary>
    public VersionStatus Status { get; set; }

    /// <summary>The title shown for this version.</summary>
    public string Title { get; set; }

    /// <summary>The URL slug for this version.</summary>
    public string Slug { get; set; }

    /// <summary>Serialized page fields, including the document and meta values.</summary>
    public string Fields { get; set; }

    /// <summary>The page this version belongs to.</summary>
    public virtual Page Page { get; set; }
}

/// <summary>Where a <see cref="PageVersion"/> sits in its page's history.</summary>
public enum VersionStatus : byte
{
    /// <summary>Saved, and not the version a public request renders.</summary>
    Draft = 0,

    /// <summary>The version a public request renders for its culture.</summary>
    Published = 1,

    /// <summary>A former version kept in the page history.</summary>
    Archived = 2
}

public class PageVersionMap : InfernoEntityTypeConfiguration<PageVersion>
{
    public override void Configure(EntityTypeBuilder<PageVersion> builder)
    {
        builder.ToTable(CmsConstants.Tables.PageVersions, InfernoSchema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PageId).IsRequired();
        builder.Property(x => x.CultureCode).HasMaxLength(10).IsUnicode(false);
        builder.Property(x => x.DateCreatedUtc).IsRequired();
        builder.Property(x => x.DateModifiedUtc).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.Fields).IsUnicode(true);

        builder.HasOne(x => x.Page).WithMany(x => x.Versions).HasForeignKey(x => x.PageId);

        builder.HasIndex(x => x.PageId);
    }
}