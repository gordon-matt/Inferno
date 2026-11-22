using Extenso.Data.Entity;
using Localize;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

/// <summary>Shared state for a content block, including the values serialized with it.</summary>
public abstract class ContentBlockBase : BaseEntity<Guid>, IContentBlock
{
    #region IContentBlock Members

    /// <inheritdoc />
    public string Title { get; set; }

    /// <inheritdoc />
    public int Order { get; set; }

    /// <inheritdoc />
    public bool Enabled { get; set; }

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.Model.ZoneId)]
    public Guid ZoneId { get; set; }

    /// <inheritdoc />
    public Guid? PageId { get; set; }

    /// <inheritdoc />
    public bool Localized { get; set; }

    /// <summary>Culture stored in the serialized block values.</summary>
    public string CultureCode { get; set; }

    /// <summary>Optional identifier stored in the serialized block values.</summary>
    public Guid? RefId { get; set; }

    /// <inheritdoc />
    public abstract Type EditorType { get; }

    /// <inheritdoc />
    public abstract Type DisplayType { get; }

    /// <inheritdoc />
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.Model.CustomTemplatePath)]
    public string CustomDisplayType { get; set; }

    #endregion IContentBlock Members
}