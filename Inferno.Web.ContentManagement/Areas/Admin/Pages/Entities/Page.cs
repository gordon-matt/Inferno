using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;

/// <summary>A page in a tenant. The published text lives on <see cref="Versions"/>.</summary>
public class Page : TenantEntity<Guid>
{
    /// <summary>The parent page. <see langword="null"/> for a top-level page.</summary>
    public Guid? ParentId { get; set; }

    /// <summary>The name shown in the admin page list.</summary>
    public string Name { get; set; }

    /// <summary>Whether the page can be viewed. A disabled page is hidden from menus and public requests.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Sort position among siblings.</summary>
    public int Order { get; set; }

    /// <summary>Whether the page is included in generated menus.</summary>
    public bool ShowOnMenus { get; set; }

    /// <summary>
    /// JSON limiting the page to role ids, or <see langword="null"/> when everyone may view it.
    /// <see cref="Inferno.Web.ContentManagement.Areas.Admin.Pages.PageSecurityHelper"/> reads and writes the value.
    /// </summary>
    public string AccessRestrictions { get; set; }

    /// <summary>The page's drafts, published versions, and archived versions.</summary>
    public ICollection<PageVersion> Versions
    {
        get => field ??= new HashSet<PageVersion>();
        set;
    }
}

public class PageMap : InfernoEntityTypeConfiguration<Page>
{
    public override void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.ToTable(CmsConstants.Tables.Pages, InfernoSchema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.Order).IsRequired();
        builder.Property(x => x.ShowOnMenus).IsRequired();
        builder.Property(x => x.AccessRestrictions).HasMaxLength(1024).IsUnicode(false);
    }
}