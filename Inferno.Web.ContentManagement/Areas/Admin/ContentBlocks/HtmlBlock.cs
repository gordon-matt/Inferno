using Inferno.Localization.ComponentModel;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks
{
    public class HtmlBlock : ContentBlockBase
    {
        #region IContentBlock Members

        public override string Name => "Html Content Block";

        public override Type EditorType => typeof(HtmlBlockEditor);

        public override Type DisplayType => typeof(HtmlBlockDisplay);

        #endregion IContentBlock Members

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.HtmlBlock.BodyContent)]
        [LocalizedHelpText(InfernoCmsLocalizableStrings.ContentBlocks.HtmlBlock.HelpText.BodyContent)]
        public string BodyContent { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.HtmlBlock.Script)]
        [LocalizedHelpText(InfernoCmsLocalizableStrings.ContentBlocks.HtmlBlock.HelpText.Script)]
        public string Script { get; set; }
    }
}