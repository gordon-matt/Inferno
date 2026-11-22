using Ardalis.Result;
using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Web.Configuration;
using Inferno.Web.Configuration.Entities;
using Inferno.Web.Configuration.Services;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Areas.Admin.Configuration.Services;

/// <summary>A registered settings group and, when one has been saved, its stored row.</summary>
/// <param name="Type">The settings type's full name.</param>
/// <param name="Name">The label from <see cref="ISettings.Name"/>.</param>
/// <param name="EditorType">The editor component.</param>
/// <param name="Stored">The saved row, or <see langword="null"/> when this tenant has not saved one.</param>
public sealed record SettingsGroup(string Type, string Name, Type EditorType, Setting Stored);

/// <summary>Lists and saves the settings documents shown on the settings screen.</summary>
public interface ISettingsAdminService
{
    /// <summary>
    /// Every registered settings group, saved or not, so a new site can configure all of them.
    /// Two registrations of the same type keep the first. The list is ordered by name.
    /// </summary>
    /// <param name="tenantId">The tenant whose stored rows are attached.</param>
    /// <returns>The groups.</returns>
    Task<Result<IReadOnlyList<SettingsGroup>>> ListAsync(int tenantId);

    /// <summary>The stored row for <paramref name="type"/>, or an unsaved one the editor can start from.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="type">The settings type's full name.</param>
    /// <returns>The row, or not found when no registered settings type has that name.</returns>
    Task<Result<Setting>> GetAsync(int tenantId, string type);

    /// <summary>
    /// Stores <paramref name="value"/> for <paramref name="type"/>. Saving site settings also drops the cached default culture.
    /// </summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="type">The settings type's full name.</param>
    /// <param name="value">The JSON to store.</param>
    /// <returns>Success, not found when the type is not registered, or an error when the save throws.</returns>
    Task<Result> SaveAsync(int tenantId, string type, string value);
}

/// <inheritdoc cref="ISettingsAdminService" />

public class SettingsAdminService(
    IEnumerable<ISettings> registeredSettings,
    IRepository<Setting> repository,
    ISettingService settingService,
    ICacheManager cacheManager,
    ILogger<SettingsAdminService> logger) : ISettingsAdminService
{
    private static readonly string SiteSettingsType = typeof(SiteSettings).FullName;

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<SettingsGroup>>> ListAsync(int tenantId)
    {
        Dictionary<string, Setting> stored;
        using (var connection = repository.OpenConnection())
        {
            stored = connection.Query(setting => setting.TenantId == tenantId)
                .ToList()
                .GroupBy(setting => setting.Type)
                .ToDictionary(group => group.Key, group => group.First());
        }

        IReadOnlyList<SettingsGroup> groups = registeredSettings
            .GroupBy(settings => settings.GetType().FullName)
            .Select(group => group.First())
            .Select(settings => new SettingsGroup(
                settings.GetType().FullName,
                settings.Name,
                settings.EditorType,
                stored.GetValueOrDefault(settings.GetType().FullName)))
            .OrderBy(group => group.Name)
            .ToList();

        return Task.FromResult(Result.Success(groups));
    }

    /// <inheritdoc />
    public async Task<Result<Setting>> GetAsync(int tenantId, string type)
    {
        var settings = registeredSettings.FirstOrDefault(candidate => candidate.GetType().FullName == type);
        if (settings is null)
        {
            return Result<Setting>.NotFound();
        }

        var stored = await repository.FindOneAsync(new SearchOptions<Setting>
        {
            Query = setting => setting.TenantId == tenantId && setting.Type == type,
        });

        return Result.Success(stored ?? new Setting { TenantId = tenantId, Name = settings.Name, Type = type });
    }

    /// <inheritdoc />
    public Task<Result> SaveAsync(int tenantId, string type, string value)
    {
        if (!registeredSettings.Any(candidate => candidate.GetType().FullName == type))
        {
            return Task.FromResult(Result.NotFound());
        }

        try
        {
            settingService.SaveSettings(type, value, tenantId);

            // The culture resolver caches the site's default language separately from the settings.
            if (type == SiteSettingsType)
            {
                cacheManager.Remove(InfernoWebConstants.CacheKeys.CurrentCulture);
            }

            return Task.FromResult(Result.Success());
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not save settings {SettingsType} for tenant {TenantId}", type, tenantId);
            return Task.FromResult(Result.Error("The settings could not be saved."));
        }
    }
}
