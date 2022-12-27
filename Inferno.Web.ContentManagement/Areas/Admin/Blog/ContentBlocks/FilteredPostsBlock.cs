using Inferno.Localization.ComponentModel;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.ContentBlocks
{
    public class FilteredPostsBlock : ContentBlockBase
    {
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.AllPostsBlock.CategoryId)]
        public int? CategoryId { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.AllPostsBlock.TagId)]
        public int? TagId { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.AllPostsBlock.FilterType)]
        public FilterType FilterType { get; set; }

        #region ContentBlockBase Overrides

        public override string Name => "Blog: Filtered Posts";

        public override string DisplayTemplatePath => "Inferno.Web.ContentManagement.Areas.Admin.Blog.Views.Shared.DisplayTemplates.FilteredPostsBlock.cshtml";

        public override string EditorTemplatePath => "Inferno.Web.ContentManagement.Areas.Admin.Blog.Views.Shared.EditorTemplates.FilteredPostsBlock.cshtml";

        #endregion ContentBlockBase Overrides
    }

    public enum FilterType : byte
    {
        None = 0,
        Category = 1,
        Tag = 2
    }
}