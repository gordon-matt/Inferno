using System.ComponentModel.DataAnnotations;
using Localize;
using Inferno.Web.Areas.Admin.Configuration.Components;

namespace Inferno.Web.Configuration;

/// <summary>The default time zone and whether a user may choose another.</summary>
public class DateTimeSettings : ISettings
{
    /// <summary>The time zone id used when a user has not chosen one.</summary>
    [Required]
    [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.DateTime.DefaultTimeZoneId)]
    public string DefaultTimeZoneId { get; set; }

    /// <summary>Whether a user may store a time zone of their own.</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.Settings.DateTime.AllowUsersToSetTimeZone)]
    public bool AllowUsersToSetTimeZone { get; set; }

    #region ISettings Members

    /// <inheritdoc />
    public string Name => "Date/Time Settings";

    /// <inheritdoc />
    public bool IsTenantRestricted => false;

    /// <inheritdoc />
    public Type EditorType => typeof(DateTimeSettingsEditor);

    //public string EditorTemplatePath => "Inferno.Web.Views.Shared.EditorTemplates.DateTimeSettings.cshtml";

    #endregion ISettings Members
}