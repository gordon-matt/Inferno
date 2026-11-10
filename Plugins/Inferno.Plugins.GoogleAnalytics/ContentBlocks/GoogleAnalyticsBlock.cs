using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

namespace Inferno.Plugins.GoogleAnalytics.ContentBlocks;

public class GoogleAnalyticsBlock : ContentBlockBase
{
    public override string Name => "Google Analytics";

    public override Type EditorType => typeof(GoogleAnalyticsBlockEditor);

    public override Type DisplayType => typeof(GoogleAnalyticsBlockDisplay);

    /// <summary>When set, this block's tag is used instead of the measurement ID in the plugin settings.</summary>
    public string? MeasurementId { get; set; }
}
