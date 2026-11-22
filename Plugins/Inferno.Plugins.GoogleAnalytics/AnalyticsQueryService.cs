using System.Globalization;
using Inferno.Web;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Inferno.Plugins.GoogleAnalytics;

/// <summary>Users and page views for one day.</summary>
/// <param name="Date">The day. <see cref="DateTime.MinValue"/> is dropped when the report date cannot be parsed.</param>
/// <param name="Users">Active users.</param>
/// <param name="Views">Page views.</param>
public sealed record AnalyticsDay(DateTime Date, int Users, int Views);

/// <summary>One ranked label, such as a page, source, or device.</summary>
/// <param name="Label">The primary text. Missing dimension values become a fallback such as "Untitled" or "Unknown".</param>
/// <param name="Value">The metric used to rank the row.</param>
/// <param name="Detail">Optional secondary text, such as a page path. <see langword="null"/> when the report has no second dimension.</param>
public sealed record AnalyticsRank(string Label, int Value, string? Detail = null);

/// <summary>The dashboard's visitor, page, source, and device numbers for one tenant.</summary>
public sealed class AnalyticsBundle
{
    /// <summary>Whether a property id and a readable service-account key were both present.</summary>
    public bool IsConfigured { get; init; }

    /// <summary>Why the report is empty. <see langword="null"/> when the numbers were loaded.</summary>
    public string? Error { get; init; }

    /// <summary>Where the user edits the missing settings. Defaults to the plugin settings page.</summary>
    public string SettingsUrl { get; init; } = GoogleAnalyticsPlugin.SettingsUrl;

    /// <summary>Active users today.</summary>
    public int TodayUsers { get; init; }

    /// <summary>Active users over the last 7 days.</summary>
    public int WeekUsers { get; init; }

    /// <summary>Active users over the last 28 days.</summary>
    public int MonthUsers { get; init; }

    /// <summary>Page views today.</summary>
    public int TodayViews { get; init; }

    /// <summary>Page views over the last 7 days.</summary>
    public int WeekViews { get; init; }

    /// <summary>Page views over the last 28 days.</summary>
    public int MonthViews { get; init; }

    /// <summary>Daily users and views for up to the last 90 days. Days whose date did not parse are omitted.</summary>
    public IReadOnlyList<AnalyticsDay> Days { get; init; } = [];

    /// <summary>The most viewed pages over the last 28 days, at most 8.</summary>
    public IReadOnlyList<AnalyticsRank> TopPages { get; init; } = [];

    /// <summary>Session channel groups over the last 28 days, at most 6.</summary>
    public IReadOnlyList<AnalyticsRank> Sources { get; init; } = [];

    /// <summary>Device categories over the last 28 days, at most 6.</summary>
    public IReadOnlyList<AnalyticsRank> Devices { get; init; } = [];

    /// <summary>A bundle that tells the widget to ask for a property id and a service-account key.</summary>
    /// <returns>A bundle with <see cref="IsConfigured"/> false.</returns>
    public static AnalyticsBundle NotConfigured() => new()
    {
        IsConfigured = false,
        Error = "Add a GA4 property ID and a service account key to show visitor numbers.",
    };

    /// <summary>A configured bundle whose reports failed.</summary>
    /// <param name="error">The message to show.</param>
    /// <returns>A bundle with <see cref="IsConfigured"/> true and <see cref="Error"/> set.</returns>
    public static AnalyticsBundle Failed(string error) => new() { IsConfigured = true, Error = error };
}

/// <summary>
/// Builds the dashboard bundle from the Data API. A successful bundle is cached for 10 minutes per tenant, property, and settings timestamp.
/// Calls that miss the cache share one in-flight request.
/// </summary>
public interface IAnalyticsQueryService
{
    /// <summary>The current tenant's bundle.</summary>
    /// <param name="refresh"><see langword="true"/> ignores the cache and any request already running.</param>
    /// <param name="cancellationToken">Observed by a new report request. A cached result, or a request already running, does not use it.</param>
    /// <returns>The bundle. Missing credentials return <see cref="AnalyticsBundle.NotConfigured"/>.</returns>
    Task<AnalyticsBundle> GetAsync(bool refresh, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IAnalyticsQueryService" />
public sealed class AnalyticsQueryService(
    IGoogleAnalyticsSettingsStore store,
    IGoogleAnalyticsClient client,
    IWorkContext work,
    IMemoryCache cache,
    ILogger<AnalyticsQueryService> logger) : IAnalyticsQueryService
{
    private readonly Dictionary<string, Task<AnalyticsBundle>> inflight = [];

    /// <inheritdoc />
    public Task<AnalyticsBundle> GetAsync(bool refresh, CancellationToken cancellationToken = default)
    {
        var settings = store.Get();
        string propertyId = GoogleAnalyticsIds.PropertyId(settings.PropertyId);
        if (propertyId.Length == 0 || !ServiceAccountKey.TryParse(settings.ServiceAccountJson, out var key, out _) || key is null)
        {
            return Task.FromResult(AnalyticsBundle.NotConfigured());
        }

        int tenantId = work.CurrentTenant?.Id ?? 0;
        string cacheKey = $"ga:{tenantId}:{propertyId}:{key.ClientEmail}:{settings.UpdatedUtc.Ticks}";
        if (!refresh && cache.TryGetValue(cacheKey, out AnalyticsBundle? cached) && cached is not null)
        {
            return Task.FromResult(cached);
        }

        if (!refresh && inflight.TryGetValue(cacheKey, out var pending))
        {
            return pending;
        }

        Task<AnalyticsBundle> task = null!;
        task = LoadAsync(key, propertyId, tenantId, cacheKey, cancellationToken);
        inflight[cacheKey] = task;
        return task;

        async Task<AnalyticsBundle> LoadAsync(ServiceAccountKey account, string property, int tenant, string keyName, CancellationToken token)
        {
            try
            {
                return await QueryAsync(account, property, tenant, keyName, token);
            }
            finally
            {
                if (inflight.TryGetValue(keyName, out var current) && ReferenceEquals(current, task))
                {
                    inflight.Remove(keyName);
                }
            }
        }
    }

    private async Task<AnalyticsBundle> QueryAsync(ServiceAccountKey key, string propertyId, int tenantId, string cacheKey, CancellationToken cancellationToken)
    {
        try
        {
            var summary = await client.RunAsync(key, propertyId, new AnalyticsQuery
            {
                Ranges =
                [
                    ("today", "today", "today"),
                    ("7daysAgo", "today", "week"),
                    ("28daysAgo", "today", "month"),
                ],
                Dimensions = ["dateRange"],
                Metrics = ["activeUsers", "screenPageViews"],
            }, cancellationToken);

            var days = await client.RunAsync(key, propertyId, new AnalyticsQuery
            {
                StartDate = "89daysAgo",
                EndDate = "today",
                Dimensions = ["date"],
                Metrics = ["activeUsers", "screenPageViews"],
            }, cancellationToken);

            var pages = await client.RunAsync(key, propertyId, new AnalyticsQuery
            {
                StartDate = "28daysAgo",
                EndDate = "today",
                Dimensions = ["pageTitle", "pagePath"],
                Metrics = ["screenPageViews"],
                OrderByMetric = "screenPageViews",
                Limit = 8,
            }, cancellationToken);

            var sources = await client.RunAsync(key, propertyId, new AnalyticsQuery
            {
                StartDate = "28daysAgo",
                EndDate = "today",
                Dimensions = ["sessionDefaultChannelGroup"],
                Metrics = ["sessions"],
                OrderByMetric = "sessions",
                Limit = 6,
            }, cancellationToken);

            var devices = await client.RunAsync(key, propertyId, new AnalyticsQuery
            {
                StartDate = "28daysAgo",
                EndDate = "today",
                Dimensions = ["deviceCategory"],
                Metrics = ["activeUsers"],
                OrderByMetric = "activeUsers",
                Limit = 6,
            }, cancellationToken);

            string? error = FirstError(summary, days, pages, sources, devices);
            if (error is not null)
            {
                return AnalyticsBundle.Failed(error);
            }

            var bundle = new AnalyticsBundle
            {
                IsConfigured = true,
                TodayUsers = Users(summary, "today"),
                WeekUsers = Users(summary, "week"),
                MonthUsers = Users(summary, "month"),
                TodayViews = Views(summary, "today"),
                WeekViews = Views(summary, "week"),
                MonthViews = Views(summary, "month"),
                Days = days.Rows
                    .Select(row => new AnalyticsDay(ParseDate(row.Dimensions.FirstOrDefault()), days.Metric(row, "activeUsers"), days.Metric(row, "screenPageViews")))
                    .Where(day => day.Date != default)
                    .OrderBy(day => day.Date)
                    .ToList(),
                TopPages = pages.Rows
                    .Select(row => new AnalyticsRank(row.Dimensions.ElementAtOrDefault(0) ?? "Untitled", pages.Metric(row, "screenPageViews"), row.Dimensions.ElementAtOrDefault(1)))
                    .ToList(),
                Sources = sources.Rows
                    .Select(row => new AnalyticsRank(row.Dimensions.FirstOrDefault() ?? "Unknown", sources.Metric(row, "sessions")))
                    .ToList(),
                Devices = devices.Rows
                    .Select(row => new AnalyticsRank(row.Dimensions.FirstOrDefault() ?? "Unknown", devices.Metric(row, "activeUsers")))
                    .ToList(),
            };

            cache.Set(cacheKey, bundle, TimeSpan.FromMinutes(10));
            return bundle;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(exception, "Google Analytics could not be reached for tenant {TenantId}.", tenantId);
            return AnalyticsBundle.Failed("Google Analytics could not be reached.");
        }
    }

    private static string? FirstError(params AnalyticsTable[] tables) =>
        tables.Select(table => table.Error).FirstOrDefault(error => !string.IsNullOrEmpty(error));

    private static int Users(AnalyticsTable table, string range) => Metric(table, range, "activeUsers");

    private static int Views(AnalyticsTable table, string range) => Metric(table, range, "screenPageViews");

    private static int Metric(AnalyticsTable table, string range, string metric)
    {
        var row = table.Rows.FirstOrDefault(candidate =>
            candidate.Dimensions.Any(dimension => string.Equals(dimension, range, StringComparison.OrdinalIgnoreCase)));
        row ??= table.Rows.ElementAtOrDefault(RangeIndex(range));
        return row is null ? 0 : table.Metric(row, metric);
    }

    private static int RangeIndex(string range) => range switch
    {
        "week" => 1,
        "month" => 2,
        _ => 0,
    };

    private static DateTime ParseDate(string? value) =>
        DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : default;
}
