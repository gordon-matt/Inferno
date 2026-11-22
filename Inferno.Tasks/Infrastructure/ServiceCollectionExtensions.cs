using Hangfire;
using Hangfire.SqlServer;
using Inferno.Tasks.Scheduling;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Tasks.Infrastructure;

/// <summary>Adds Hangfire for an installed site.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Background jobs for an installed site. Hangfire stores them in SQL Server, so a web farm runs each
    /// job once. Call this only when a connection string exists; before installation there is no database to use.
    /// Storage uses the HangFire schema and creates it when it is missing. <see cref="ScheduledTaskRunner"/> is registered as transient.
    /// </summary>
    /// <param name="services">The collection to add Hangfire to.</param>
    /// <param name="connectionString">SQL Server connection string for the HangFire schema.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection AddInfernoHangfire(this IServiceCollection services, string connectionString)
    {
        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                SchemaName = "HangFire",
                PrepareSchemaIfNecessary = true,
            }));

        services.AddHangfireServer();
        services.AddTransient<ScheduledTaskRunner>();
        return services;
    }
}
