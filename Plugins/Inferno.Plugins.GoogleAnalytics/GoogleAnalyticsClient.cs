using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Caching.Memory;

namespace Inferno.Plugins.GoogleAnalytics;

/// <summary>One Data API runReport request. <see cref="Ranges"/> replaces <see cref="StartDate"/> and <see cref="EndDate"/> when it has entries.</summary>
public sealed class AnalyticsQuery
{
    /// <summary>The start date when <see cref="Ranges"/> is empty. Defaults to the last 28 days.</summary>
    public string StartDate { get; init; } = "28daysAgo";

    /// <summary>The end date when <see cref="Ranges"/> is empty. Defaults to today.</summary>
    public string EndDate { get; init; } = "today";

    /// <summary>Named date ranges. <see langword="null"/> or empty uses <see cref="StartDate"/> and <see cref="EndDate"/>.</summary>
    public IReadOnlyList<(string Start, string End, string Name)>? Ranges { get; init; }

    /// <summary>Dimension names. Empty omits the dimensions field.</summary>
    public IReadOnlyList<string> Dimensions { get; init; } = [];

    /// <summary>Metric names.</summary>
    public IReadOnlyList<string> Metrics { get; init; } = [];

    /// <summary>A metric to sort by, descending. <see langword="null"/> or empty leaves the API's order.</summary>
    public string? OrderByMetric { get; init; }

    /// <summary>How many rows to ask for. <see langword="null"/> omits the limit.</summary>
    public int? Limit { get; init; }
}

/// <summary>Runs a GA4 Data API report with a service-account token. The token is cached per client email.</summary>
public interface IGoogleAnalyticsClient
{
    /// <summary>
    /// Posts <paramref name="query"/> to the property's runReport method.
    /// A token request that is refused, or a key that cannot be signed, returns a failed table instead of throwing.
    /// </summary>
    /// <param name="key">The service account that signs the token.</param>
    /// <param name="propertyId">The numeric property id.</param>
    /// <param name="query">The report to run.</param>
    /// <param name="cancellationToken">Observed by the token request and the report request.</param>
    /// <returns>The parsed table, including a failure stored on the table.</returns>
    Task<AnalyticsTable> RunAsync(ServiceAccountKey key, string propertyId, AnalyticsQuery query, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IGoogleAnalyticsClient" />
public sealed class GoogleAnalyticsClient(IHttpClientFactory http, IMemoryCache cache) : IGoogleAnalyticsClient
{
    /// <summary>The named <see cref="HttpClient"/> registered for this plugin.</summary>
    public const string HttpClientName = "Inferno.GoogleAnalytics";

    private const string Scope = "https://www.googleapis.com/auth/analytics.readonly";

    /// <inheritdoc />
    public async Task<AnalyticsTable> RunAsync(ServiceAccountKey key, string propertyId, AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        string token;
        try
        {
            token = await TokenAsync(key, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException or CryptographicException or JsonException)
        {
            return AnalyticsTable.Failed("Google did not accept the service account key.");
        }

        var body = new JsonObject
        {
            ["dateRanges"] = query.Ranges is { Count: > 0 } ranges
                ? new JsonArray(ranges.Select(range => (JsonNode)new JsonObject
                {
                    ["startDate"] = range.Start,
                    ["endDate"] = range.End,
                    ["name"] = range.Name,
                }).ToArray())
                : new JsonArray(new JsonObject { ["startDate"] = query.StartDate, ["endDate"] = query.EndDate }),
            ["metrics"] = new JsonArray(query.Metrics.Select(metric => (JsonNode)new JsonObject { ["name"] = metric }).ToArray()),
        };

        if (query.Dimensions.Count > 0)
        {
            body["dimensions"] = new JsonArray(query.Dimensions.Select(dimension => (JsonNode)new JsonObject { ["name"] = dimension }).ToArray());
        }

        if (!string.IsNullOrEmpty(query.OrderByMetric))
        {
            body["orderBys"] = new JsonArray(new JsonObject
            {
                ["metric"] = new JsonObject { ["metricName"] = query.OrderByMetric },
                ["desc"] = true,
            });
        }

        if (query.Limit is int limit)
        {
            body["limit"] = limit;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://analyticsdata.googleapis.com/v1beta/properties/{propertyId}:runReport")
        {
            Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var client = http.CreateClient(HttpClientName);
        using var response = await client.SendAsync(request, cancellationToken);
        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        return AnalyticsReportParser.Parse(json);
    }

    private async Task<string> TokenAsync(ServiceAccountKey key, CancellationToken cancellationToken)
    {
        string cacheKey = "ga-token:" + key.ClientEmail;
        if (cache.TryGetValue(cacheKey, out string? cached) && !string.IsNullOrEmpty(cached))
        {
            return cached;
        }

        var now = DateTimeOffset.UtcNow;
        string header = Base64Url("""{"alg":"RS256","typ":"JWT"}"""u8);
        string payloadJson = new JsonObject
        {
            ["iss"] = key.ClientEmail,
            ["scope"] = Scope,
            ["aud"] = key.TokenUri,
            ["iat"] = now.ToUnixTimeSeconds(),
            ["exp"] = now.AddMinutes(50).ToUnixTimeSeconds(),
        }.ToJsonString();
        string unsigned = header + "." + Base64Url(Encoding.UTF8.GetBytes(payloadJson));

        using var rsa = RSA.Create();
        rsa.ImportFromPem(key.PrivateKey);
        byte[] signature = rsa.SignData(Encoding.UTF8.GetBytes(unsigned), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        string assertion = unsigned + "." + Base64Url(signature);

        using var request = new HttpRequestMessage(HttpMethod.Post, key.TokenUri)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                ["assertion"] = assertion,
            }),
        };

        var client = http.CreateClient(HttpClientName);
        using var response = await client.SendAsync(request, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("The token request was refused.");
        }

        using var document = JsonDocument.Parse(body);
        string? token = document.RootElement.GetProperty("access_token").GetString();
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("The token response had no access token.");
        }

        int expires = document.RootElement.TryGetProperty("expires_in", out var expiresElement) ? expiresElement.GetInt32() : 3600;
        cache.Set(cacheKey, token, TimeSpan.FromSeconds(Math.Max(60, expires - 60)));
        return token;
    }

    private static string Base64Url(ReadOnlySpan<byte> data) =>
        Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
