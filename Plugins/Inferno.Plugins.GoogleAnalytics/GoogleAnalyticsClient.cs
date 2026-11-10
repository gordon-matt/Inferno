using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Caching.Memory;

namespace Inferno.Plugins.GoogleAnalytics;

public sealed class AnalyticsQuery
{
    public string StartDate { get; init; } = "28daysAgo";

    public string EndDate { get; init; } = "today";

    public IReadOnlyList<(string Start, string End, string Name)>? Ranges { get; init; }

    public IReadOnlyList<string> Dimensions { get; init; } = [];

    public IReadOnlyList<string> Metrics { get; init; } = [];

    public string? OrderByMetric { get; init; }

    public int? Limit { get; init; }
}

public interface IGoogleAnalyticsClient
{
    Task<AnalyticsTable> RunAsync(ServiceAccountKey key, string propertyId, AnalyticsQuery query, CancellationToken cancellationToken = default);
}

public sealed class GoogleAnalyticsClient(IHttpClientFactory http, IMemoryCache cache) : IGoogleAnalyticsClient
{
    public const string HttpClientName = "Inferno.GoogleAnalytics";

    private const string Scope = "https://www.googleapis.com/auth/analytics.readonly";

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
