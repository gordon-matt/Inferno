using Localize;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

/// <summary>A form posted to the CMS. The fields are emailed, or forwarded when <see cref="FormUrl"/> is set.</summary>
public class FormBlock : ContentBlockBase
{
    //[AllowHtml]
    /// <summary>HTML fields rendered inside the form. A submit button is added when the template has no button element.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.HtmlTemplate)]
    public string HtmlTemplate { get; set; }

    //[AllowHtml]
    /// <summary>Message shown after a successful submission.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.ThankYouMessage)]
    public string ThankYouMessage { get; set; }

    /// <summary>Where the visitor is sent after a successful submission. Empty sends them to the site root.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.RedirectUrl)]
    public string RedirectUrl { get; set; }

    /// <summary>Recipient when the form is emailed.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.EmailAddress)]
    [LocalizedHelpText(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.HelpText.EmailAddress)]
    public string EmailAddress { get; set; }

    /// <summary>Posts the form with script and shows the result in a dialog.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.UseAjax)]
    public bool UseAjax { get; set; }

    /// <summary>When set, the fields are posted to this URL instead of being emailed. Only a public http or https address is accepted.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.FormUrl)]
    [LocalizedHelpText(InfernoCmsLocalizableStrings.ContentBlocks.FormBlock.HelpText.FormUrl)]
    public string FormUrl { get; set; }

    #region ContentBlockBase Overrides

    /// <inheritdoc />
    public override string Name => "Form Content Block";

    /// <inheritdoc />
    public override Type EditorType => typeof(FormBlockEditor);

    /// <inheritdoc />
    public override Type DisplayType => typeof(FormBlockDisplay);

    #endregion ContentBlockBase Overrides
}