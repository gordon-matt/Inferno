using Hangfire;
using Hangfire.SqlServer;
using Inferno.Tasks.Scheduling;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Tasks.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Background jobs for an installed site. Hangfire stores them in SQL Server, so a web farm runs each
    /// job once. Call this only when a connection string exists; before installation there is no database to use.
    /// </summary>
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
