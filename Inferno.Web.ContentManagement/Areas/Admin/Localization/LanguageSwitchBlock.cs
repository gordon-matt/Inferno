using Inferno.Localization.ComponentModel;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;
using Inferno.Web.ContentManagement.Areas.Admin.Localization.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Localization
{
    public class LanguageSwitchBlock : ContentBlockBase
    {
        public enum LanguageSwitchStyle : byte
        {
            BootstrapNavbarDropdown = 0,
            Select = 1,
            List = 2
        }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.LanguageSwitchBlock.Style)]
        public LanguageSwitchStyle Style { get; set; }

        //[LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.LanguageSwitchBlock.UseUrlPrefix)]
        //public bool UseUrlPrefix { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.LanguageSwitchBlock.IncludeInvariant)]
        public bool IncludeInvariant { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.LanguageSwitchBlock.InvariantText)]
        public string InvariantText { get; set; }

        #region ContentBlockBase Overrides

        public override string Name => "Language Switch";

        public override Type EditorType => typeof(LanguageSwitchBlockEditor);

        public override Type DisplayType => typeof(LanguageSwitchBlockDisplay);

        #endregion ContentBlockBase Overrides
    }
}