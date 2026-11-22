using Localize;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.ContentBlocks;

/// <summary>Lists blog categories. Zero shows every category; any other value shows that many.</summary>
public class CategoriesBlock : ContentBlockBase
{
    /// <summary>How many categories to show. Zero shows all of them.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.CategoriesBlock.NumberOfCategories)]
    public byte NumberOfCategories { get; set; }

    #region ContentBlockBase Overrides

    /// <inheritdoc />
    public override string Name => "Blog: Categories";

    /// <inheritdoc />
    public override Type EditorType => typeof(CategoriesBlockEditor);

    /// <inheritdoc />
    public override Type DisplayType => typeof(CategoriesBlockDisplay);

    #endregion ContentBlockBase Overrides
}