using Inferno.Localization.ComponentModel;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks
{
    public class HtmlBlock : ContentBlockBase
    {
        #region IContentBlock Members

        public override string Name => "Html Content Block";

        public override string DisplayTemplatePath => "Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Views.Shared.DisplayTemplates.HtmlBlock.cshtml";

        public override string EditorTemplatePath => "Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Views.Shared.EditorTemplates.HtmlBlock.cshtml";

        #endregion IContentBlock Members

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.HtmlBlock.BodyContent)]
        [LocalizedHelpText(InfernoCmsLocalizableStrings.ContentBlocks.HtmlBlock.HelpText.BodyContent)]
        public string BodyContent { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.HtmlBlock.Script)]
        [LocalizedHelpText(InfernoCmsLocalizableStrings.ContentBlocks.HtmlBlock.HelpText.Script)]
        public string Script { get; set; }
    }
}