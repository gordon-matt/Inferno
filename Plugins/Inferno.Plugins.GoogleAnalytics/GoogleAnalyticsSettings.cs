using System.Text.Json;

namespace Inferno.Plugins.GoogleAnalytics;

public sealed class GoogleAnalyticsSettings
{
    public string MeasurementId { get; set; } = string.Empty;

    public string PropertyId { get; set; } = string.Empty;

    public string ServiceAccountJson { get; set; } = string.Empty;

    public DateTime UpdatedUtc { get; set; }
}

public sealed class ServiceAccountKey
{
    public required string ClientEmail { get; init; }

    public required string PrivateKey { get; init; }

    public required string TokenUri { get; init; }

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
