using Inferno.Localization.ComponentModel;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Components;
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

        public override Type EditorType => typeof(FilteredPostsBlockEditor);

        public override Type DisplayType => typeof(FilteredPostsBlockDisplay);

        #endregion ContentBlockBase Overrides
    }

    public enum FilterType : byte
    {
        None = 0,
        Category = 1,
        Tag = 2
    }
}