using Inferno.Localization.ComponentModel;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks
{
    public class FormBlock : ContentBlockBase
    {
        //[AllowHtml]
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.HtmlTemplate)]
        public string HtmlTemplate { get; set; }

        //[AllowHtml]
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.ThankYouMessage)]
        public string ThankYouMessage { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.RedirectUrl)]
        public string RedirectUrl { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.EmailAddress)]
        [LocalizedHelpText(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.HelpText.EmailAddress)]
        public string EmailAddress { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.UseAjax)]
        public bool UseAjax { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.FormUrl)]
        [LocalizedHelpText(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.HelpText.FormUrl)]
        public string FormUrl { get; set; }

        #region ContentBlockBase Overrides

        public override string Name => "Form Content Block";

        public override string DisplayTemplatePath => "Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Views.Shared.DisplayTemplates.FormBlock.cshtml";

        public override string EditorTemplatePath => "Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Views.Shared.EditorTemplates.FormBlock.cshtml";

        #endregion ContentBlockBase Overrides
    }
}