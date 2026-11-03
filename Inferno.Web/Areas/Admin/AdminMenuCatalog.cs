using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Navigation;
using Inferno.Web.Navigation;

namespace Inferno.Web.Areas.Admin;

/// <summary>
/// The shared "Admin" menu, built from the navigation providers. The admin shell, LogVue and
/// IdentityNexus all read this, so plugins appear on every back office page.
/// Registered scoped because providers read the current tenant and language.
/// </summary>
public sealed class AdminMenuCatalog(INavigationManager navigation) : INetForgeMenuCatalog
{
    public const string MenuName = "Admin";

    private NetForgeMenuModel? admin;

    public IReadOnlyCollection<string> Names { get; } = [MenuName];

    public NetForgeMenuModel Get(string? name) =>
        string.Equals(name?.Trim(), MenuName, StringComparison.OrdinalIgnoreCase)
            ? admin ??= Build()
            : NetForgeMenuModel.Empty(name);

    private NetForgeMenuModel Build()
    {
        var sections = navigation.BuildMenu(InfernoWebConstants.Areas.Admin)
            .Select(ToSection)
            .ToList();

        return new NetForgeMenuModel(MenuName, true, NetForgeMenuPlacement.Topbar, sections);
    }

    private ForgeNavSection ToSection(MenuItem item)
    {
        var children = item.Items?.ToList() ?? [];

        if (children.Count == 0)
        {
            return new ForgeNavSection { Items = [ToItem(item)] };
        }

        return new ForgeNavSection
        {
            Heading = item.Text,
            Items = children.Select(ToItem).ToList(),
        };
    }

    private ForgeNavItem ToItem(MenuItem item)
    {
        string href = navigation.GetUrl(item.Href ?? item.Url) ?? string.Empty;

        return new ForgeNavItem
        {
            Title = item.Text ?? string.Empty,
            Href = href,
            Icon = Icon(item.IconName),
            External = item.IsExternal,
            MatchAll = href.Equals("/admin", StringComparison.OrdinalIgnoreCase),
            Children = item.Items?.Select(ToItem).ToList() ?? [],
        };
    }

    private static string Icon(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "circle";
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Providers may name a NetForge icon directly; legacy Font Awesome class lists are translated below.
        if (!parts.Any(part => part is "fa" or "fas" or "far" or "fab" or "fal"))
        {
            return name.Trim();
        }

        string key = parts[^1];

        if (key.StartsWith("fa-", StringComparison.OrdinalIgnoreCase))
        {
            key = key[3..];
        }

        return key.ToLowerInvariant() switch
        {
            "tachometer-alt" => "layout",
            "users" => "people",
            "star" => "star",
            "language" => "globe",
            "puzzle-piece" => "layout",
            "building" => "briefcase",
            "tint" => "droplet",
            "wrench" => "settings",
            "exclamation-triangle" => "warning",
            "clock" => "clock",
            "edit" => "edit",
            "paperclip" => "link",
            "file" => "logs",
            "folder" => "archive",
            "tag" => "bookmark",
            "square" => "rectangle",
            "th-large" => "layout",
            "image" => "film",
            "arrow-right" => "chevron-right",
            "code" => "code",
            "envelope" => "inbox",
            "inbox" => "inbox",
            "map" => "signpost",
            _ => "circle",
        };
    }
}
