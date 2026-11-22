using Localize;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;
using Inferno.Web.ContentManagement.Areas.Admin.Localization.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Localization;

/// <summary>Lets a visitor change the current culture.</summary>
public class LanguageSwitchBlock : ContentBlockBase
{
    /// <summary>How the language choices are drawn.</summary>
    public enum LanguageSwitchStyle : byte
    {
        /// <summary>A Bootstrap navbar dropdown.</summary>
        BootstrapNavbarDropdown = 0,

        /// <summary>A select element.</summary>
        Select = 1,

        /// <summary>A list of buttons.</summary>
        List = 2
    }

    /// <summary>Which <see cref="LanguageSwitchStyle"/> to render. Anything other than the dropdown or select is rendered as a list.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.LanguageSwitchBlock.Style)]
    public LanguageSwitchStyle Style { get; set; }

    //[LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.LanguageSwitchBlock.UseUrlPrefix)]
    //public bool UseUrlPrefix { get; set; }

    /// <summary>Adds the invariant culture as the first choice.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.LanguageSwitchBlock.IncludeInvariant)]
    public bool IncludeInvariant { get; set; }

    /// <summary>Label for the invariant choice. Blank is shown as "[ Invariant ]".</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.LanguageSwitchBlock.InvariantText)]
    public string InvariantText { get; set; }

    #region ContentBlockBase Overrides

    /// <inheritdoc />
    public override string Name => "Language Switch";

    /// <inheritdoc />
    public override Type EditorType => typeof(LanguageSwitchBlockEditor);

    /// <inheritdoc />
    public override Type DisplayType => typeof(LanguageSwitchBlockDisplay);

    #endregion ContentBlockBase Overrides
}