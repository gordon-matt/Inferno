using Localize;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.ContentBlocks;

/// <summary>Lists up to 20 recent posts, optionally limited to one category or tag.</summary>
public class FilteredPostsBlock : ContentBlockBase
{
    /// <summary>The category used when <see cref="FilterType"/> is <see cref="FilterType.Category"/>.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.AllPostsBlock.CategoryId)]
    public int? CategoryId { get; set; }

    /// <summary>The tag used when <see cref="FilterType"/> is <see cref="FilterType.Tag"/>.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.AllPostsBlock.TagId)]
    public int? TagId { get; set; }

    /// <summary>Whether to list every recent post, one category, or one tag.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.AllPostsBlock.FilterType)]
    public FilterType FilterType { get; set; }

    #region ContentBlockBase Overrides

    /// <inheritdoc />
    public override string Name => "Blog: Filtered Posts";

    /// <inheritdoc />
    public override Type EditorType => typeof(FilteredPostsBlockEditor);

    /// <inheritdoc />
    public override Type DisplayType => typeof(FilteredPostsBlockDisplay);

    #endregion ContentBlockBase Overrides
}

/// <summary>How a <see cref="FilteredPostsBlock"/> chooses its posts.</summary>
public enum FilterType : byte
{
    /// <summary>The 20 most recent posts.</summary>
    None = 0,

    /// <summary>Posts in <see cref="FilteredPostsBlock.CategoryId"/>. A missing category shows nothing.</summary>
    Category = 1,

    /// <summary>Posts with <see cref="FilteredPostsBlock.TagId"/>. A missing tag shows nothing.</summary>
    Tag = 2
}