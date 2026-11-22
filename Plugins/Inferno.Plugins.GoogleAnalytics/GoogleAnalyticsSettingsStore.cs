using System.Text.Json;
using Extenso.Data.Entity;
using Inferno.Web;
using Inferno.Web.Configuration.Entities;

namespace Inferno.Plugins.GoogleAnalytics;

/// <summary>Loads and stores <see cref="GoogleAnalyticsSettings"/> for the current tenant.</summary>
public interface IGoogleAnalyticsSettingsStore
{
    /// <summary>The current tenant's settings. A missing row, blank JSON, or JSON that does not parse returns a new instance.</summary>
    /// <returns>The settings. Never <see langword="null"/>.</returns>
    GoogleAnalyticsSettings Get();

    /// <summary>Inserts or updates the current tenant's settings and sets <see cref="GoogleAnalyticsSettings.UpdatedUtc"/>.</summary>
    /// <param name="settings">The values to store.</param>
    void Save(GoogleAnalyticsSettings settings);
}

/// <inheritdoc cref="IGoogleAnalyticsSettingsStore" />
public sealed class GoogleAnalyticsSettingsStore(IRepository<Setting> settings, IWorkContext work) : IGoogleAnalyticsSettingsStore
{
    /// <summary>The settings type key stored in the settings table. This is not an <see cref="Inferno.Web.Configuration.ISettings"/> type.</summary>
    public const string TypeName = "Inferno.Plugins.GoogleAnalytics.Settings";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <inheritdoc />
    public GoogleAnalyticsSettings Get()
    {
        int? tenantId = work.CurrentTenant?.Id;
        var stored = settings.FindOne(new SearchOptions<Setting>
        {
            Query = setting => setting.TenantId == tenantId && setting.Type == TypeName,
        });

        if (stored is null || string.IsNullOrWhiteSpace(stored.Value))
        {
            return new GoogleAnalyticsSettings();
        }

        try
        {
            return JsonSerializer.Deserialize<GoogleAnalyticsSettings>(stored.Value, JsonOptions) ?? new GoogleAnalyticsSettings();
        }
        catch (JsonException)
        {
            return new GoogleAnalyticsSettings();
        }
    }

    /// <inheritdoc />
    public void Save(GoogleAnalyticsSettings value)
    {
        int? tenantId = work.CurrentTenant?.Id;
        value.UpdatedUtc = DateTime.UtcNow;
        string json = JsonSerializer.Serialize(value, JsonOptions);
        var stored = settings.FindOne(new SearchOptions<Setting>
        {
            Query = setting => setting.TenantId == tenantId && setting.Type == TypeName,
        });

        if (stored is null)
        {
            settings.Insert(new Setting
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Google Analytics",
                Type = TypeName,
                Value = json,
            });
            return;
        }

        stored.Value = json;
        settings.Update(stored);
    }
}
