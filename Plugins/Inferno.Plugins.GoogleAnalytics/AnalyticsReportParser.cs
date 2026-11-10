using System.Text.Json;

namespace Inferno.Plugins.GoogleAnalytics;

public sealed record AnalyticsTableRow(IReadOnlyList<string> Dimensions, IReadOnlyList<string> Metrics);

public sealed record AnalyticsTable(
    IReadOnlyList<string> Dimensions,
    IReadOnlyList<string> Metrics,
    IReadOnlyList<AnalyticsTableRow> Rows,
    string? Error)
{
    public bool Succeeded => Error is null;

    public static AnalyticsTable Failed(string error) => new([], [], [], error);

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

public static class AnalyticsReportParser
{
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
