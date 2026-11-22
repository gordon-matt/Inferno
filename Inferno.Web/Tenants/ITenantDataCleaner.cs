namespace Inferno.Web.Tenants;

/// <summary>Deletes the content, mail, settings, users, and media stored for one tenant. The tenant row itself is left for the caller.</summary>
public interface ITenantDataCleaner
{
    /// <summary>
    /// Deletes the tenant's blog, menus, pages, blocks, mail, settings, localization rows, users, roles, upload folder, and sitemap file.
    /// A missing blog factory or a missing localization catalog is skipped.
    /// </summary>
    /// <param name="tenantId">The tenant whose data is removed.</param>
    /// <returns>A task that completes when the deletes have finished.</returns>
    Task CleanAsync(int tenantId);
}
