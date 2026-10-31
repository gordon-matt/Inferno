using Inferno.Tenants.Entities;

namespace Inferno.Tenants;

public static class TenantUrlExtensions
{
    /// <summary>
    /// Public origin for generated links. When SSL is enabled, <see cref="Tenant.SecureUrl"/> wins over the current request.
    /// </summary>
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
