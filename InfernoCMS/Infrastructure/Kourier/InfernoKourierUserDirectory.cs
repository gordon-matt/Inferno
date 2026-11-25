using System.Globalization;
using Inferno.Identity;
using Inferno.Security.Membership;
using Inferno.Web.Security.Membership;
using Kourier.Core;

namespace InfernoCMS.Infrastructure.Kourier;

/// <summary>
/// Host users for Kourier campaigns, previews and user tokens, loaded through Inferno membership
/// and scoped to the current tenant.
/// </summary>
public sealed class InfernoKourierUserDirectory(
    IMembershipService membership,
    IKourierTenantScope tenantScope) : IKourierUserDirectory
{
    /// <inheritdoc />
    public async Task<KourierUser?> FindAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var user = await membership.GetUserByIdAsync(userId);
        if (user is null || !BelongsToCurrentTenant(user))
        {
            return null;
        }

        return await ToKourierUserAsync(user);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<KourierUser>> SearchAsync(string? query, int take, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 100);
        var users = await membership.GetAllUsersAsync(CurrentTenant());
        string needle = query?.Trim() ?? string.Empty;
        IEnumerable<InfernoUser> filtered = string.IsNullOrEmpty(needle)
            ? users
            : users.Where(user =>
                (user.Email?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false)
                || (user.UserName?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false));

        var page = filtered
            .OrderBy(user => user.Email ?? user.UserName ?? user.Id?.ToString() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Take(take)
            .ToList();

        var result = new List<KourierUser>(page.Count);
        foreach (var user in page)
        {
            result.Add(await ToKourierUserAsync(user));
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<KourierUserPage> GetPageAsync(string? cursor, int take, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 500);
        var users = (await membership.GetAllUsersAsync(CurrentTenant()))
            .OrderBy(user => user.Id?.ToString() ?? string.Empty, StringComparer.Ordinal)
            .ToList();

        int start = 0;
        if (!string.IsNullOrWhiteSpace(cursor))
        {
            int index = users.FindIndex(user => string.Equals(user.Id?.ToString(), cursor, StringComparison.Ordinal));
            start = index < 0 ? users.Count : index + 1;
        }

        var slice = users.Skip(start).Take(take).ToList();
        var mapped = new List<KourierUser>(slice.Count);
        foreach (var user in slice)
        {
            mapped.Add(await ToKourierUserAsync(user));
        }

        string? next = slice.Count < take ? null : slice[^1].Id?.ToString();
        return new KourierUserPage(mapped, next);
    }

    private int? CurrentTenant()
    {
        string? tenantId = tenantScope.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return CurrentTenantId.Resolve();
        }

        return int.TryParse(tenantId, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) ? id : null;
    }

    private bool BelongsToCurrentTenant(InfernoUser user)
    {
        int? current = CurrentTenant();
        return current is null ? user.TenantId is null : user.TenantId == current;
    }

    private async Task<KourierUser> ToKourierUserAsync(InfernoUser user)
    {
        string id = user.Id?.ToString() ?? string.Empty;
        var profile = string.IsNullOrEmpty(id)
            ? new Dictionary<string, string>()
            : await membership.GetProfileAsync(id);

        profile.TryGetValue(AccountUserProfileProvider.Fields.GivenNames, out string? given);
        profile.TryGetValue(AccountUserProfileProvider.Fields.FamilyName, out string? family);
        string? display = await membership.GetUserDisplayNameAsync(user);

        IReadOnlyDictionary<string, string>? properties = profile.Count == 0
            ? null
            : new Dictionary<string, string>(profile);

        return new KourierUser(
            id,
            user.Email ?? string.Empty,
            display,
            given,
            family,
            Culture: null,
            EmailConfirmed: user.EmailConfirmed,
            Properties: properties);
    }
}