using Inferno.Localization.ComponentModel;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.ContentBlocks
{
    public class LastNPostsBlock : ContentBlockBase
    {
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.LastNPostsBlock.NumberOfEntries)]
        public byte NumberOfEntries { get; set; }

        #region ContentBlockBase Overrides

        public override string Name => "Blog: Last (N) Posts";

        public override string DisplayTemplatePath => "Inferno.Web.ContentManagement.Areas.Admin.Blog.Views.Shared.DisplayTemplates.LastNPostsBlock.cshtml";

        public override string EditorTemplatePath => "Inferno.Web.ContentManagement.Areas.Admin.Blog.Views.Shared.EditorTemplates.LastNPostsBlock.cshtml";

        #endregion ContentBlockBase Overrides
    }
}