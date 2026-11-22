using Dependo;
using Extenso;
using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Web.Configuration.Entities;

namespace Inferno.Web.Configuration.Services;

/// <summary>
/// Loads and stores settings JSON. Results are cached by tenant and type until a save for that tenant clears them.
/// A <see langword="null"/> tenant id reads and writes the row whose tenant is <see langword="null"/>.
/// </summary>
public interface ISettingService
{
    /// <summary>Loads <typeparamref name="TSettings"/> for <paramref name="tenantId"/>, or a new instance when nothing is stored.</summary>
    /// <typeparam name="TSettings">The settings type.</typeparam>
    /// <param name="tenantId">The tenant, or <see langword="null"/> for the row with no tenant.</param>
    /// <returns>The stored settings, or a new instance when the row is missing or its value is empty.</returns>
    TSettings GetSettings<TSettings>(int? tenantId = null) where TSettings : ISettings, new();

    /// <summary>Loads <paramref name="settingsType"/> for <paramref name="tenantId"/>, or a new instance when nothing is stored.</summary>
    /// <param name="settingsType">The settings type. Its full name is the stored type key.</param>
    /// <param name="tenantId">The tenant, or <see langword="null"/> for the row with no tenant.</param>
    /// <returns>The stored settings, or a new instance when the row is missing or its value is empty.</returns>
    ISettings GetSettings(Type settingsType, int? tenantId = null);

    /// <summary>
    /// Inserts or updates the JSON stored under <paramref name="key"/>.
    /// An insert happens only when a registered <see cref="ISettings"/> has that full type name; otherwise nothing is written.
    /// A write clears the cached settings for <paramref name="tenantId"/>.
    /// </summary>
    /// <param name="key">The settings type's full name.</param>
    /// <param name="value">The JSON to store.</param>
    /// <param name="tenantId">The tenant, or <see langword="null"/> for the row with no tenant.</param>
    void SaveSettings(string key, string value, int? tenantId = null);

    /// <summary>Serializes <paramref name="settings"/> and stores it under its runtime type name.</summary>
    /// <typeparam name="TSettings">The settings type.</typeparam>
    /// <param name="settings">The object to serialize.</param>
    /// <param name="tenantId">The tenant, or <see langword="null"/> for the row with no tenant.</param>
    void SaveSettings<TSettings>(TSettings settings, int? tenantId = null) where TSettings : ISettings;
}

/// <inheritdoc cref="ISettingService" />
public class DefaultSettingService : ISettingService
{
    private readonly ICacheManager cacheManager;
    private readonly IRepository<Setting> repository;

    /// <summary>Creates the service.</summary>
    /// <param name="cacheManager">Cache of settings, keyed by tenant and type.</param>
    /// <param name="repository">The settings table.</param>
    public DefaultSettingService(ICacheManager cacheManager, IRepository<Setting> repository)
    {
        this.cacheManager = cacheManager;
        this.repository = repository;
    }

    /// <inheritdoc />
    public TSettings GetSettings<TSettings>(int? tenantId = null) where TSettings : ISettings, new()
    {
        string type = typeof(TSettings).FullName;
        string key = string.Format(InfernoWebConstants.CacheKeys.SettingsKeyFormat, tenantId, type);
        return cacheManager.Get(key, () =>
        {
            var settings = tenantId.HasValue
                ? repository.FindOne(new SearchOptions<Setting>
                {
                    Query = x => x.TenantId == tenantId && x.Type == type
                })
                : repository.FindOne(new SearchOptions<Setting>
                {
                    Query = x => x.TenantId == null && x.Type == type
                });
            return settings == null || string.IsNullOrEmpty(settings.Value) ? new TSettings() : settings.Value.JsonDeserialize<TSettings>();
        });
    }

    /// <inheritdoc />
    public ISettings GetSettings(Type settingsType, int? tenantId = null)
    {
        string type = settingsType.FullName;
        string key = string.Format(InfernoWebConstants.CacheKeys.SettingsKeyFormat, tenantId, type);
        return cacheManager.Get(key, () =>
        {
            var settings = tenantId.HasValue
                ? repository.FindOne(new SearchOptions<Setting>
                {
                    Query = x => x.TenantId == tenantId && x.Type == type
                })
                : repository.FindOne(new SearchOptions<Setting>
                {
                    Query = x => x.TenantId == null && x.Type == type
                });
            return settings == null || string.IsNullOrEmpty(settings.Value)
                ? (ISettings)Activator.CreateInstance(settingsType)
                : (ISettings)settings.Value.JsonDeserialize(settingsType);
        });
    }

    /// <inheritdoc />
    public void SaveSettings(string key, string value, int? tenantId = null)
    {
        var setting = tenantId.HasValue
            ? repository.FindOne(new SearchOptions<Setting>
            {
                Query = x => x.TenantId == tenantId && x.Type == key
            })
            : repository.FindOne(new SearchOptions<Setting>
            {
                Query = x => x.TenantId == null && x.Type == key
            });
        if (setting == null)
        {
            var iSettings = DependoResolver.Instance.ResolveAll<ISettings>().FirstOrDefault(x => x.GetType().FullName == key);

            if (iSettings != null)
            {
                setting = new Setting { TenantId = tenantId, Name = iSettings.Name, Type = key, Value = value };
                repository.Insert(setting);
                cacheManager.RemoveByPattern(string.Format(InfernoWebConstants.CacheKeys.SettingsKeysPatternFormat, tenantId));
            }
        }
        else
        {
            setting.Value = value;
            repository.Update(setting);
            cacheManager.RemoveByPattern(string.Format(InfernoWebConstants.CacheKeys.SettingsKeysPatternFormat, tenantId));
        }
    }

    /// <inheritdoc />
    public void SaveSettings<TSettings>(TSettings settings, int? tenantId = null) where TSettings : ISettings
    {
        var type = settings.GetType();
        string key = type.FullName;
        string value = settings.JsonSerialize();
        SaveSettings(key, value, tenantId);
    }
}