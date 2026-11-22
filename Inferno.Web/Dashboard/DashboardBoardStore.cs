using Extenso.Data.Entity;
using GridKrafter.Models;
using GridKrafter.Serialization;
using Inferno.Web.Configuration.Entities;

namespace Inferno.Web.Dashboard;

/// <summary>Loads and stores one person's dashboard arrangement in the settings table.</summary>
public interface IDashboardBoardStore
{
    /// <summary>The saved board for <paramref name="userId"/> in <paramref name="tenantId"/>.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="userId">The user. Whitespace returns <see langword="null"/>.</param>
    /// <returns>The board, or <see langword="null"/> when nothing is stored or the JSON cannot be read.</returns>
    DashboardBoard Load(int tenantId, string userId);

    /// <summary>Inserts or updates the board. A blank <paramref name="userId"/> or a null <paramref name="board"/> does nothing.</summary>
    /// <param name="tenantId">The tenant the row is stored under.</param>
    /// <param name="userId">The user. Combined with <paramref name="tenantId"/> as the settings type key.</param>
    /// <param name="board">The arrangement to serialize.</param>
    void Save(int tenantId, string userId, DashboardBoard board);
}

/// <summary>
/// Keeps each person's arrangement in the settings table. The type key is not an <c>ISettings</c> type,
/// so it stays out of the settings screen.
/// </summary>
public sealed class DashboardBoardStore(IRepository<Setting> settings) : IDashboardBoardStore
{
    private const string KeyPrefix = "Inferno.Dashboard:";

    /// <inheritdoc />
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

    /// <inheritdoc />
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
