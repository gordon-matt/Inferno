using Extenso.Data.Entity;
using Inferno.Localization.ComponentModel;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks
{
    public abstract class ContentBlockBase : BaseEntity<Guid>, IContentBlock
    {
        #region IContentBlock Members

        public string Title { get; set; }

        public int Order { get; set; }

        public bool Enabled { get; set; }

        public abstract string Name { get; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.Model.ZoneId)]
        public Guid ZoneId { get; set; }

        public Guid? PageId { get; set; }

        public bool Localized { get; set; }

        public string CultureCode { get; set; }

        public Guid? RefId { get; set; }

        public abstract Type EditorType { get; }

        public abstract Type DisplayType { get; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.Model.CustomTemplatePath)]
        public string CustomDisplayType { get; set; }

        #endregion IContentBlock Members
    }
}