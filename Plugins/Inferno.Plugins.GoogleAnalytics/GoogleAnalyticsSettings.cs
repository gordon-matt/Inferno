using System.Text.Json;

namespace Inferno.Plugins.GoogleAnalytics;

/// <summary>The measurement id, GA4 property, and service-account key stored for the current tenant.</summary>
public sealed class GoogleAnalyticsSettings
{
    /// <summary>The measurement id sent with pages. Empty when the site has not set one.</summary>
    public string MeasurementId { get; set; } = string.Empty;

    /// <summary>The GA4 property id, digits only, used by the dashboard.</summary>
    public string PropertyId { get; set; } = string.Empty;

    /// <summary>The service-account JSON key. Empty when the dashboard has no credentials.</summary>
    public string ServiceAccountJson { get; set; } = string.Empty;

    /// <summary>When these settings were last saved, in UTC. Included in the report cache key.</summary>
    public DateTime UpdatedUtc { get; set; }
}

/// <summary>The fields of a Google service-account key that the Data API client signs with.</summary>
public sealed class ServiceAccountKey
{
    /// <summary>The service-account email. Also the issuer of the signed token.</summary>
    public required string ClientEmail { get; init; }

    /// <summary>The PEM private key.</summary>
    public required string PrivateKey { get; init; }

    /// <summary>The token endpoint. A blank value in the JSON becomes <c>https://oauth2.googleapis.com/token</c>.</summary>
    public required string TokenUri { get; init; }

    /// <summary>
    /// Reads <paramref name="json"/> as a service-account key.
    /// A missing client email or private key, and JSON that does not parse, both fail.
    /// </summary>
    /// <param name="json">The key file contents.</param>
    /// <param name="key">The key when this returns <see langword="true"/>; otherwise <see langword="null"/>.</param>
    /// <param name="error">A message when this returns <see langword="false"/>; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when <paramref name="key"/> is ready to sign.</returns>
    public static bool TryParse(string? json, out ServiceAccountKey? key, out string? error)
    {
        key = null;
        error = null;
        if (string.IsNullOrWhiteSpace(json))
        {
            error = "Add the service account key.";
            return false;
        }

        try
        {
            var document = JsonSerializer.Deserialize<ServiceAccountDocument>(json);
            if (document is null || string.IsNullOrWhiteSpace(document.ClientEmail) || string.IsNullOrWhiteSpace(document.PrivateKey))
            {
                error = "The key needs a client email and a private key.";
                return false;
            }

            key = new ServiceAccountKey
            {
                ClientEmail = document.ClientEmail.Trim(),
                PrivateKey = document.PrivateKey,
                TokenUri = string.IsNullOrWhiteSpace(document.TokenUri) ? "https://oauth2.googleapis.com/token" : document.TokenUri.Trim(),
            };
            return true;
        }
        catch (JsonException)
        {
            error = "The service account key is not valid JSON.";
            return false;
        }
    }

    private sealed class ServiceAccountDocument
    {
        public string? client_email { get; set; }

        public string? private_key { get; set; }

        public string? token_uri { get; set; }

        public string? ClientEmail => client_email;

        public string? PrivateKey => private_key;

        public string? TokenUri => token_uri;
    }
}
