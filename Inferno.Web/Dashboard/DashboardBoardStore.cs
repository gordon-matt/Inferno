using Extenso.Data.Entity;
using GridKrafter.Models;
using GridKrafter.Serialization;
using Inferno.Web.Configuration.Entities;

namespace Inferno.Web.Dashboard;

public interface IDashboardBoardStore
{
    DashboardBoard Load(int tenantId, string userId);

    void Save(int tenantId, string userId, DashboardBoard board);
}

/// <summary>
/// Keeps each person's arrangement in the settings table. The type key is not an <c>ISettings</c> type,
/// so it stays out of the settings screen.
/// </summary>
public sealed class DashboardBoardStore(IRepository<Setting> settings) : IDashboardBoardStore
{
    private const string KeyPrefix = "Inferno.Dashboard:";

    public DashboardBoard Load(int tenantId, string userId)
    {
        var stored = Find(tenantId, userId);
        if (stored is null || string.IsNullOrWhiteSpace(stored.Value))
        {
            return null;
        }

        var parsed = DashboardJson.TryParse(stored.Value);
        return parsed.IsSuccess ? parsed.Value : null;
    }

    public void Save(int tenantId, string userId, DashboardBoard board)
    {
        if (string.IsNullOrWhiteSpace(userId) || board is null)
        {
            return;
        }

        string json = DashboardJson.Serialize(board);
        var stored = Find(tenantId, userId);
        if (stored is null)
        {
            settings.Insert(new Setting
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Dashboard",
                Type = Key(tenantId, userId),
                Value = json,
            });
            return;
        }

        stored.Value = json;
        settings.Update(stored);
    }

    private Setting Find(int tenantId, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        string key = Key(tenantId, userId);
        return settings.FindOne(new SearchOptions<Setting>
        {
            Query = setting => setting.TenantId == tenantId && setting.Type == key,
        });
    }

    private static string Key(int tenantId, string userId) => $"{KeyPrefix}{tenantId}:{userId}";
}
