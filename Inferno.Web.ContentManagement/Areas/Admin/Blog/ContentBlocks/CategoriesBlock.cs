using Inferno.Localization.ComponentModel;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.ContentBlocks
{
    public class CategoriesBlock : ContentBlockBase
    {
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.CategoriesBlock.NumberOfCategories)]
        public byte NumberOfCategories { get; set; }

        #region ContentBlockBase Overrides

        public override string Name => "Blog: Categories";

        public override Type EditorType => typeof(CategoriesBlockEditor);

        public override Type DisplayType => typeof(CategoriesBlockDisplay);

        #endregion ContentBlockBase Overrides
    }
}