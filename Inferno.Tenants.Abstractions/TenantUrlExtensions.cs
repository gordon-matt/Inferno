using Inferno.Tenants.Entities;

namespace Inferno.Tenants;

/// <summary>Builds the origin used in generated links.</summary>
public static class TenantUrlExtensions
{
    /// <summary>
    /// Public origin for generated links. When SSL is enabled, <see cref="Tenant.SecureUrl"/> wins over the current request.
    /// </summary>
    /// <param name="tenant">The tenant whose secure address is preferred. <see langword="null"/>, or a tenant that is not using SSL, falls through to <paramref name="requestBaseUrl"/>.</param>
    /// <param name="requestBaseUrl">The current request origin. <see langword="null"/> is treated as empty. A trailing slash is removed.</param>
    /// <returns>
    /// <see cref="Tenant.SecureUrl"/> trimmed, with <c>https://</c> added when it has no scheme, when SSL is enabled and that value is set.
    /// Otherwise <paramref name="requestBaseUrl"/> without a trailing slash.
    /// </returns>
    public static string GetPublicBaseUrl(this Tenant tenant, string requestBaseUrl)
    {
        if (tenant != null && tenant.SslEnabled && !string.IsNullOrWhiteSpace(tenant.SecureUrl))
        {
            string secure = tenant.SecureUrl.Trim().TrimEnd('/');
            if (!secure.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !secure.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                secure = "https://" + secure;
            }

            return secure;
        }

        return (requestBaseUrl ?? string.Empty).TrimEnd('/');
    }
}
