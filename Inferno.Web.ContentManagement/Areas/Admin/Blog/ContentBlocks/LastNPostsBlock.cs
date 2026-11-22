using Localize;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.ContentBlocks;

/// <summary>Lists the most recent blog posts.</summary>
public class LastNPostsBlock : ContentBlockBase
{
    /// <summary>How many posts to show.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.LastNPostsBlock.NumberOfEntries)]
    public byte NumberOfEntries { get; set; }

    #region ContentBlockBase Overrides

    /// <inheritdoc />
    public override string Name => "Blog: Last (N) Posts";

    /// <inheritdoc />
    public override Type EditorType => typeof(LastNPostsBlockEditor);

    /// <inheritdoc />
    public override Type DisplayType => typeof(LastNPostsBlockDisplay);

    #endregion ContentBlockBase Overrides
}