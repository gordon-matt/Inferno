using Extenso.Data.Entity;
using Inferno.Web.Infrastructure;
using InfernoCMS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InfernoCMS.Services;

/// <summary>
/// Opens an <see cref="ApplicationDbContext"/> from the saved data settings, the application connection string, or an in-memory database.
/// </summary>
public class DataSettingsDbContextFactory : IDbContextFactory
{
    private readonly ApplicationDbContextFactory inner;

    /// <summary>Creates the inner factory from <paramref name="configuration"/>.</summary>
    /// <param name="configuration">Application configuration used when data settings have no connection string.</param>
    public DataSettingsDbContextFactory(IConfiguration configuration)
    {
        inner = new ApplicationDbContextFactory(configuration);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Uses the connection string saved by installation when one exists. Otherwise uses the application connection string.
    /// When that is also missing, returns an in-memory database named InfernoCMS.
    /// </remarks>
    public DbContext GetContext()
    {
        var settings = DataSettingsManager.LoadSettings();
        if (!string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            return inner.GetContext(settings.ConnectionString);
        }

        if (string.IsNullOrWhiteSpace(inner.AppSettingsConnectionString))
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("InfernoCMS")
                .Options;
            return new ApplicationDbContext(options);
        }

        return inner.GetContext();
    }

    /// <inheritdoc />
    public DbContext GetContext(string connectionString) => inner.GetContext(connectionString);
}
