using Localize;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

/// <summary>A block of HTML, with an optional script.</summary>
public class HtmlBlock : ContentBlockBase
{
    #region IContentBlock Members

    /// <inheritdoc />
    public override string Name => "Html Content Block";

    /// <inheritdoc />
    public override Type EditorType => typeof(HtmlBlockEditor);

    /// <inheritdoc />
    public override Type DisplayType => typeof(HtmlBlockDisplay);

    #endregion IContentBlock Members

    /// <summary>The HTML rendered for the block.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.HtmlBlock.BodyContent)]
    [LocalizedHelpText(InfernoCmsLocalizableStrings.ContentBlocks.HtmlBlock.HelpText.BodyContent)]
    public string BodyContent { get; set; }

    /// <summary>Script rendered with the block.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.HtmlBlock.Script)]
    [LocalizedHelpText(InfernoCmsLocalizableStrings.ContentBlocks.HtmlBlock.HelpText.Script)]
    public string Script { get; set; }
}