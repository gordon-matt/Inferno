using Inferno.Web.Security;
using Microsoft.Extensions.DependencyInjection;
using Localize;
using Localize.Data.Sql;

namespace Inferno.Web.Localization;

public static class CmsLocalizationExtensions
{
    /// <summary>
    /// Registers the shared localization catalog. Pass the site connection string when the database
    /// is installed. Without one, language packs still resolve and the admin stays unavailable.
    /// </summary>
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
