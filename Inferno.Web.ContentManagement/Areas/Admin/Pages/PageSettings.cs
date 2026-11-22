using Localize;
using Inferno.Web.Configuration;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages;

/// <summary>How much page history each tenant keeps.</summary>
public class PageSettings : ISettings
{
    /// <summary>Sets <see cref="NumberOfPageVersionsToKeep"/> to 5.</summary>
    public PageSettings()
    {
        NumberOfPageVersionsToKeep = 5;
    }

    #region ISettings Members

    /// <inheritdoc />
    public string Name => "CMS: Page Settings";

    /// <inheritdoc />
    public bool IsTenantRestricted => false;

    /// <inheritdoc />
    public Type EditorType => typeof(PageSettingsEditor);

    #endregion ISettings Members

    /// <summary>How many earlier saves of each page to keep. Saving a page keeps at least one.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.Settings.Pages.NumberOfPageVersionsToKeep)]
    public short NumberOfPageVersionsToKeep { get; set; }
}