using Inferno.Tenants.Entities;

namespace Inferno.Tenants;

/// <summary>Reads the comma-separated hosts stored on a <see cref="Tenant"/>.</summary>
public static class TenantExtensions
{
    /// <summary>Splits <see cref="Tenant.Hosts"/> on commas and trims each value. Empty pieces are dropped.</summary>
    /// <param name="tenant">The tenant whose hosts are read. Must not be <see langword="null"/>.</param>
    /// <returns>The host values. Empty when <see cref="Tenant.Hosts"/> is <see langword="null"/> or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tenant"/> is <see langword="null"/>.</exception>
    public static IEnumerable<string> ParseHostValues(this Tenant tenant)
    {
        if (tenant == null)
        {
            throw new ArgumentNullException(nameof(tenant));
        }

        var parsedValues = new List<string>();
        if (!string.IsNullOrEmpty(tenant.Hosts))
        {
            string[] hosts = tenant.Hosts.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string host in hosts)
            {
                string tmp = host.Trim();
                if (!string.IsNullOrEmpty(tmp))
                {
                    parsedValues.Add(tmp);
                }
            }
        }
        return parsedValues;
    }

    /// <summary>Reports whether <paramref name="host"/> is one of <paramref name="tenant"/>'s host values, ignoring case.</summary>
    /// <param name="tenant">The tenant to test. Must not be <see langword="null"/>.</param>
    /// <param name="host">The host to look for. <see langword="null"/> or empty returns <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when a stored host equals <paramref name="host"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tenant"/> is <see langword="null"/>.</exception>
    public static bool ContainsHostValue(this Tenant tenant, string host) => tenant == null
            ? throw new ArgumentNullException(nameof(tenant))
            : !string.IsNullOrEmpty(host) && tenant.ParseHostValues().Any(x => x.Equals(host, StringComparison.OrdinalIgnoreCase));
}
