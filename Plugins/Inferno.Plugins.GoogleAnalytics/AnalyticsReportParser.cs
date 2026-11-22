using System.Text.Json;

namespace Inferno.Plugins.GoogleAnalytics;

/// <summary>One row from a Data API report.</summary>
/// <param name="Dimensions">Dimension values, in header order.</param>
/// <param name="Metrics">Metric values, in header order, still as text.</param>
public sealed record AnalyticsTableRow(IReadOnlyList<string> Dimensions, IReadOnlyList<string> Metrics);

/// <summary>A parsed Data API report, or the error Google returned.</summary>
/// <param name="Dimensions">Dimension header names.</param>
/// <param name="Metrics">Metric header names.</param>
/// <param name="Rows">The data rows. Empty when the report failed.</param>
/// <param name="Error">The failure message. <see langword="null"/> when the report was read.</param>
public sealed record AnalyticsTable(
    IReadOnlyList<string> Dimensions,
    IReadOnlyList<string> Metrics,
    IReadOnlyList<AnalyticsTableRow> Rows,
    string? Error)
{
    /// <summary>Whether <see cref="Error"/> is <see langword="null"/>.</summary>
    public bool Succeeded => Error is null;

    /// <summary>A table whose only content is <paramref name="error"/>.</summary>
    /// <param name="error">The failure message.</param>
    /// <returns>A failed table.</returns>
    public static AnalyticsTable Failed(string error) => new([], [], [], error);

    /// <summary>The integer metric named <paramref name="name"/> on <paramref name="row"/>.</summary>
    /// <param name="row">The row to read.</param>
    /// <param name="name">The metric header. The match is ordinal.</param>
    /// <returns>The parsed integer, or 0 when the metric is missing or not an integer.</returns>
    public int Metric(AnalyticsTableRow row, string name)
    {
        int index = Metrics.ToList().FindIndex(metric => string.Equals(metric, name, StringComparison.Ordinal));
        if (index < 0 || index >= row.Metrics.Count)
        {
            return 0;
        }

        return int.TryParse(row.Metrics[index], out int value) ? value : 0;
    }
}

/// <summary>Reads a Data API runReport JSON payload.</summary>
public static class AnalyticsReportParser
{
    /// <summary>
    /// Parses <paramref name="json"/>. An empty body, a Google <c>error</c> object, and JSON that does not parse
    /// all return a failed table.
    /// </summary>
    /// <param name="json">The response body.</param>
    /// <returns>The table. Failures are stored on <see cref="AnalyticsTable.Error"/> rather than thrown.</returns>
    public static AnalyticsTable Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return AnalyticsTable.Failed("Google Analytics returned an empty response.");
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.TryGetProperty("error", out var error))
            {
                string message = error.TryGetProperty("message", out var messageElement)
                    ? messageElement.GetString() ?? "Google Analytics refused the request."
                    : "Google Analytics refused the request.";
                return AnalyticsTable.Failed(message);
            }

            var rows = new List<AnalyticsTableRow>();
            if (root.TryGetProperty("rows", out var rowArray))
            {
                foreach (var row in rowArray.EnumerateArray())
                {
                    rows.Add(new AnalyticsTableRow(Values(row, "dimensionValues"), Values(row, "metricValues")));
                }
            }

            return new AnalyticsTable(Names(root, "dimensionHeaders"), Names(root, "metricHeaders"), rows, null);
        }
        catch (JsonException)
        {
            return AnalyticsTable.Failed("Google Analytics returned a response that could not be read.");
        }
    }

    private static IReadOnlyList<string> Names(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var headers))
        {
            return [];
        }

        return headers.EnumerateArray()
            .Select(header => header.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty)
            .ToList();
    }

    private static IReadOnlyList<string> Values(JsonElement row, string property)
    {
        if (!row.TryGetProperty(property, out var values))
        {
            return [];
        }

        return values.EnumerateArray()
            .Select(value => value.TryGetProperty("value", out var text) ? text.GetString() ?? string.Empty : string.Empty)
            .ToList();
    }
}
