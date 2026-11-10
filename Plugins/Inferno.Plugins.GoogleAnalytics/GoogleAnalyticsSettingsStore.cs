using System.Text.Json;
using Extenso.Data.Entity;
using Inferno.Web;
using Inferno.Web.Configuration.Entities;

namespace Inferno.Plugins.GoogleAnalytics;

public interface IGoogleAnalyticsSettingsStore
{
    GoogleAnalyticsSettings Get();

    void Save(GoogleAnalyticsSettings settings);
}

public sealed class GoogleAnalyticsSettingsStore(IRepository<Setting> settings, IWorkContext work) : IGoogleAnalyticsSettingsStore
{
    public const string TypeName = "Inferno.Plugins.GoogleAnalytics.Settings";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

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
