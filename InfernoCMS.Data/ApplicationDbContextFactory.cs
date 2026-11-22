using Extenso.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InfernoCMS.Data;

/// <inheritdoc cref="IDbContextFactory" />
public class ApplicationDbContextFactory : IDbContextFactory
{
    private readonly IConfiguration configuration;

    /// <summary>Stores <paramref name="configuration"/> for the default SQL Server connection.</summary>
    /// <param name="configuration">Application configuration. The default connection is named DefaultConnection.</param>
    public ApplicationDbContextFactory(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    /// <summary>The DefaultConnection string, or <see langword="null"/> when configuration has none.</summary>
    public string AppSettingsConnectionString => configuration.GetConnectionString("DefaultConnection");

    private DbContextOptions<ApplicationDbContext> Options
    {
        get
        {
            if (field == null)
            {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                field = optionsBuilder.Options;
            }
            return field;
        }
    }

    /// <inheritdoc />
    /// <remarks>The context uses SQL Server and the DefaultConnection connection string.</remarks>
    public DbContext GetContext() => new ApplicationDbContext(Options);

    /// <inheritdoc />
    /// <remarks>The context uses SQL Server.</remarks>
    public DbContext GetContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}