using Inferno.Tasks.Scheduling;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Inferno.Web.Areas.Admin.Home.Pages;

public partial class Index
{
    private sealed record DashboardTile(string Href, string Icon, string Title, string Subtitle = null, bool FullReload = false, bool NewTab = false);

    [Inject]
    private AdminLinkAccess Links { get; set; }

    [Inject]
    private AuthenticationStateProvider AuthenticationState { get; set; }

    private static readonly DashboardTile[] contentTiles =
    [
        new("/admin/pages/index", "code", InfernoWebLocalizableStrings.Dashboard.Cards.Pages, InfernoWebLocalizableStrings.Dashboard.Cards.PagesSubtitle),
        new("/admin/blog/posts", "link", InfernoWebLocalizableStrings.Dashboard.Cards.Blog, InfernoWebLocalizableStrings.Dashboard.Cards.BlogSubtitle),
        new("/admin/menus/index", "menu", InfernoWebLocalizableStrings.Dashboard.Cards.Menus, InfernoWebLocalizableStrings.Dashboard.Cards.MenusSubtitle),
        new("/admin/blocks/content-blocks", "grid", InfernoWebLocalizableStrings.Dashboard.Cards.ContentBlocks, InfernoWebLocalizableStrings.Dashboard.Cards.ContentBlocksSubtitle),
        new("/admin/media/index", "image", InfernoWebLocalizableStrings.Dashboard.Cards.Media, InfernoWebLocalizableStrings.Dashboard.Cards.MediaSubtitle)
    ];

    // Membership is served by its own router, so a full page load is needed to reach it.
    private static readonly DashboardTile[] adminTiles =
    [
        new("/admin/membership", "users", InfernoWebLocalizableStrings.Membership.Title, InfernoWebLocalizableStrings.Dashboard.Cards.MembershipSubtitle, FullReload: true),
        new("/admin/configuration/settings/index", "settings", InfernoWebLocalizableStrings.General.Settings, InfernoWebLocalizableStrings.Dashboard.Cards.SettingsSubtitle),
        new("/admin/themes", "palette", InfernoWebLocalizableStrings.General.Themes, InfernoWebLocalizableStrings.Dashboard.Cards.ThemesSubtitle),
        new("/admin/tenants/index", "briefcase", InfernoWebLocalizableStrings.Tenants.Title, InfernoWebLocalizableStrings.Dashboard.Cards.TenantsSubtitle)
    ];

    private static readonly DashboardTile[] maintenanceTiles =
    [
        new(ScheduledJobs.DashboardPath, "clock", InfernoWebLocalizableStrings.ScheduledTasks.Title, InfernoWebLocalizableStrings.Dashboard.Cards.ScheduledTasksSubtitle, NewTab: true),
        new("/logvue", "logs", InfernoWebLocalizableStrings.Log.Title),
        new("/localize/languages", "translate", InfernoWebLocalizableStrings.Localization.Title),
        new("/pluginz", "puzzle", InfernoWebLocalizableStrings.Plugins.Title)
    ];

    private IReadOnlyList<DashboardTile> visibleContent = [];
    private IReadOnlyList<DashboardTile> visibleAdmin = [];
    private IReadOnlyList<DashboardTile> visibleMaintenance = [];

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthenticationState.GetAuthenticationStateAsync();
        var allowed = await Links.VisibleHrefsAsync(state.User);
        visibleContent = Allowed(contentTiles, allowed);
        visibleAdmin = Allowed(adminTiles, allowed);
        visibleMaintenance = Allowed(maintenanceTiles, allowed);
    }

    private static DashboardTile[] Allowed(IEnumerable<DashboardTile> tiles, IReadOnlySet<string> hrefs) =>
        [.. tiles.Where(tile => hrefs.Contains(AdminLinkAccess.Key(tile.Href)))];
}