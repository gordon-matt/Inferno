using Extenso.Data.Entity;
using Inferno.Web.Infrastructure;
using InfernoCMS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InfernoCMS.Services;

public class DataSettingsDbContextFactory : IDbContextFactory
{
    private readonly ApplicationDbContextFactory inner;

    public DataSettingsDbContextFactory(IConfiguration configuration)
    {
        inner = new ApplicationDbContextFactory(configuration);
    }

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

    public DbContext GetContext(string connectionString) => inner.GetContext(connectionString);
}
