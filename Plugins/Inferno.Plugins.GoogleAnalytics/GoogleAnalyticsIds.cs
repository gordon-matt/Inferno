using System.Text.RegularExpressions;

namespace Inferno.Plugins.GoogleAnalytics;

public static partial class GoogleAnalyticsIds
{
    public static string NormalizeMeasurementId(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

    public static bool IsMeasurementId(string? value) => MeasurementIdPattern().IsMatch(NormalizeMeasurementId(value));

    /// <summary>The block's own id when it is set, otherwise the id saved in the plugin settings.</summary>
    public static string EffectiveMeasurementId(string? blockValue, string? settingsValue)
    {
        string block = NormalizeMeasurementId(blockValue);
        if (IsMeasurementId(block))
        {
            return block;
        }

        string settings = NormalizeMeasurementId(settingsValue);
        return IsMeasurementId(settings) ? settings : string.Empty;
    }

    /// <summary>Accepts <c>123</c> or <c>properties/123</c>.</summary>
    public static string PropertyId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string trimmed = value.Trim();
        const string prefix = "properties/";
        if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[prefix.Length..];
        }

        return trimmed.Length > 0 && trimmed.All(char.IsDigit) ? trimmed : string.Empty;
    }

    [GeneratedRegex("^G-[A-Z0-9]+$", RegexOptions.CultureInvariant)]
    private static partial Regex MeasurementIdPattern();
}
