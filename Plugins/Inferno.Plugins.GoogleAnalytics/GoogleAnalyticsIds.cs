using System.Text.RegularExpressions;

namespace Inferno.Plugins.GoogleAnalytics;

/// <summary>Checks and normalizes GA4 measurement ids and property ids.</summary>
public static partial class GoogleAnalyticsIds
{
    /// <summary>Trims <paramref name="value"/> and uppercases it. Whitespace returns empty.</summary>
    /// <param name="value">The measurement id as entered.</param>
    /// <returns>The normalized id, or empty.</returns>
    public static string NormalizeMeasurementId(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

    /// <summary>Whether <paramref name="value"/> is a <c>G-</c> measurement id after <see cref="NormalizeMeasurementId"/>.</summary>
    /// <param name="value">The measurement id as entered.</param>
    /// <returns><see langword="true"/> when the normalized value matches <c>G-</c> followed by letters and digits.</returns>
    public static bool IsMeasurementId(string? value) => MeasurementIdPattern().IsMatch(NormalizeMeasurementId(value));

    /// <summary>The block's own id when it is set, otherwise the id saved in the plugin settings.</summary>
    /// <param name="blockValue">The id stored on a content block.</param>
    /// <param name="settingsValue">The id stored in the plugin settings.</param>
    /// <returns>The first value that is a measurement id, or empty when neither is.</returns>
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
    /// <param name="value">The property id as entered.</param>
    /// <returns>The digits, or empty when the value is blank or not all digits after the optional prefix.</returns>
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
