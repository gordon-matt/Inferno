using Inferno.Web.Security;
using Microsoft.Extensions.DependencyInjection;
using Localize;
using Localize.Data.Sql;

namespace Inferno.Web.Localization;

/// <summary>Service registration for the shared localization catalog.</summary>
public static class CmsLocalizationExtensions
{
    /// <summary>
    /// Registers the shared localization catalog. Pass the site connection string when the database
    /// is installed. Without one, language packs still resolve and the admin stays unavailable.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The site database. Whitespace skips the SQL Server catalog.</param>
    /// <returns>The same collection, so calls can be chained.</returns>
    public static IServiceCollection AddCmsLocalization(this IServiceCollection services, string? connectionString)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddLocalizeSqlServer(connectionString);
        }

        services.AddLocalize<TenantLocalizationScope>(options =>
        {
            options.TrackMissingKeys = true;
            options.ManagePolicy = InfernoWebPolicies.LanguagesWrite;
        });

        return services;
    }
}
