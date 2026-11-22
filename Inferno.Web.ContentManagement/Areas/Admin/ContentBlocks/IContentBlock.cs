namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

/// <summary>A content block that can be stored, edited, and rendered in a zone.</summary>
public interface IContentBlock
{
    /// <summary>The stored block's identifier.</summary>
    Guid Id { get; set; }

    /// <summary>The block's title.</summary>
    string Title { get; set; }

    /// <summary>Sort position within the zone. The zone renders blocks in this order.</summary>
    int Order { get; set; }

    /// <summary>Whether the block is shown. Disabled blocks are omitted unless a loader is asked to include them.</summary>
    bool Enabled { get; set; }

    /// <summary>The name shown for this block type.</summary>
    string Name { get; }

    /// <summary>The zone that contains the block.</summary>
    Guid ZoneId { get; set; }

    /// <summary>The page this block belongs to. <see langword="null"/> when the block belongs only to the zone.</summary>
    Guid? PageId { get; set; }

    /// <summary>Serialized with the block values.</summary>
    bool Localized { get; set; }

    /// <summary>The editor component for this block type.</summary>
    Type EditorType { get; }

    /// <summary>The component that renders this block.</summary>
    Type DisplayType { get; }

    /// <summary>Optional path of a template that replaces <see cref="DisplayType"/>.</summary>
    string CustomDisplayType { get; set; }
}