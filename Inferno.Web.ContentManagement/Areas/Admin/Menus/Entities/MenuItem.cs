using Extenso.Data.Entity;
using Inferno.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;

/// <summary>One entry in a <see cref="Menu"/>.</summary>
public class MenuItem : BaseEntity<Guid>
{
    /// <summary>The menu this item belongs to.</summary>
    public Guid MenuId { get; set; }

    /// <summary>The link text.</summary>
    public string Text { get; set; }

    /// <summary>Optional description stored with the item.</summary>
    public string Description { get; set; }

    /// <summary>The link address.</summary>
    public string Url { get; set; }

    /// <summary>Optional CSS class applied to the item.</summary>
    public string CssClass { get; set; }

    /// <summary>Sort position among siblings. Lower values come first.</summary>
    public int Position { get; set; }

    /// <summary>The parent item. <see langword="null"/> for a top-level item.</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Whether the item is shown.</summary>
    public bool Enabled { get; set; }

    /// <summary>Whether <see cref="Url"/> points outside this website.</summary>
    public bool IsExternalUrl { get; set; }

    /// <summary>Opens the link in a new browser tab. Not stored; set by code that builds a menu.</summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool OpenInNewTab { get; set; }

    /// <summary>Optional identifier of another record this item points at.</summary>
    public Guid? RefId { get; set; }

    //public virtual Menu Menu { get; set; }

    //public virtual MenuItem Parent { get; set; }

    /// <summary>Returns <see cref="Text"/>.</summary>
    /// <returns>The link text.</returns>
    public override string ToString() => Text;
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
        builder.Ignore(x => x.OpenInNewTab);

        //HasRequired(x => x.Menu).WithMany().HasForeignKey(x => x.MenuId);
        //HasOptional(x => x.Parent).WithMany().HasForeignKey(x => x.ParentId);
    }
}