using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

namespace Inferno.Plugins.GoogleAnalytics.ContentBlocks;

/// <summary>Injects the Google Analytics tag. <see cref="MeasurementId"/> overrides the id saved in the plugin settings.</summary>
public class GoogleAnalyticsBlock : ContentBlockBase
{
    /// <inheritdoc />
    public override string Name => "Google Analytics";

    /// <inheritdoc />
    public override Type EditorType => typeof(GoogleAnalyticsBlockEditor);

    /// <inheritdoc />
    public override Type DisplayType => typeof(GoogleAnalyticsBlockDisplay);

    /// <summary>When set, this block's tag is used instead of the measurement ID in the plugin settings.</summary>
    public string? MeasurementId { get; set; }
}
