using Inferno.Localization.ComponentModel;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.ContentBlocks
{
    public class CategoriesBlock : ContentBlockBase
    {
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.CategoriesBlock.NumberOfCategories)]
        public byte NumberOfCategories { get; set; }

        #region ContentBlockBase Overrides

        public override string Name => "Blog: Categories";

        public override string DisplayTemplatePath => "Inferno.Web.ContentManagement.Areas.Admin.Blog.Views.Shared.DisplayTemplates.CategoriesBlock.cshtml";

        public override string EditorTemplatePath => "Inferno.Web.ContentManagement.Areas.Admin.Blog.Views.Shared.EditorTemplates.CategoriesBlock.cshtml";

        #endregion ContentBlockBase Overrides
    }
}