using Inferno.Localization.ComponentModel;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.ContentBlocks
{
    public class LastNPostsBlock : ContentBlockBase
    {
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.LastNPostsBlock.NumberOfEntries)]
        public byte NumberOfEntries { get; set; }

        #region ContentBlockBase Overrides

        public override string Name => "Blog: Last (N) Posts";

        public override Type EditorType => typeof(LastNPostsBlockEditor);

        public override Type DisplayType => typeof(LastNPostsBlockDisplay);

        #endregion ContentBlockBase Overrides
    }
}