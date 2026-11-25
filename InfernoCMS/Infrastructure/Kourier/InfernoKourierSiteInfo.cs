using System.Globalization;
using Inferno.Tenants;
using Inferno.Tenants.Entities;
using Inferno.Web.Configuration;
using Inferno.Web.Configuration.Services;
using Kourier.Core;
using Microsoft.Extensions.Options;

namespace InfernoCMS.Infrastructure.Kourier;

/// <summary>Site name and public URL from the Inferno tenant and its site settings.</summary>
public sealed class InfernoKourierSiteInfo(
    IRepository<Tenant> tenants,
    ISettingService settings,
    IOptions<KourierOptions> options) : IKourierSiteInfo
{
    /// <inheritdoc />
    public async Task<KourierSiteInfo> GetAsync(string? tenantId, CancellationToken cancellationToken = default)
    {
        Tenant? tenant = null;
        if (!string.IsNullOrWhiteSpace(tenantId)
            && int.TryParse(tenantId, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
        {
            tenant = await tenants.FindOneAsync(id);
        }

        var site = settings.GetSettings<SiteSettings>(tenant?.Id);
        string name = !string.IsNullOrWhiteSpace(site.SiteName)
            ? site.SiteName
            : tenant?.Name ?? options.Value.SiteName ?? "Site";

        string url = ResolveUrl(tenant) ?? options.Value.SiteUrl?.TrimEnd('/') ?? string.Empty;
        return new KourierSiteInfo(name, url, options.Value.LogoUrl, options.Value.PostalAddress);
    }

    private static string? ResolveUrl(Tenant? tenant)
    {
        if (tenant is null)
        {
            return null;
        }

        string fromSecure = tenant.GetPublicBaseUrl(tenant.Url);
        if (!string.IsNullOrWhiteSpace(fromSecure))
        {
            return fromSecure.TrimEnd('/');
        }

        string raw = (tenant.Url ?? string.Empty).Trim().TrimEnd('/');
        if (raw.Length == 0)
        {
            return null;
        }

        if (!raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            raw = "https://" + raw;
        }

        return raw;
    }
}