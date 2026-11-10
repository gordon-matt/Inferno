using System.Text.Json.Nodes;
using GridKrafter.Components;
using GridKrafter.Models;
using Inferno.Web.ContentManagement.Areas.Admin.Dashboard.Widgets;
using Inferno.Web.Dashboard;

namespace Inferno.Web.ContentManagement.Areas.Admin.Dashboard;

public static class DashboardKinds
{
    public const string Stat = "inferno.stat";

    public const string Activity = "inferno.activity";

    public const string Mix = "inferno.mix";

    public const string Recent = "inferno.recent";

    public const string Scheduled = "inferno.scheduled";

    public const string Attention = "inferno.attention";

    public const string Roles = "inferno.roles";

    public const string Accounts = "inferno.accounts";

    public const string Shortcuts = "inferno.shortcuts";
}

public static class DashboardMetrics
{
    public const string PublishedPages = "published-pages";

    public const string PublishedPosts = "published-posts";

    public const string Drafts = "drafts";

    public const string ScheduledPosts = "scheduled-posts";

    public const string Attention = "attention";

    public const string Users = "users";
}

public sealed class ContentDashboardContributor : IDashboardContributor
{
    public int Order => 0;

    public IReadOnlyList<WidgetDefinition> Widgets { get; } =
    [
        WidgetDefinition.For<StatWidget>(DashboardKinds.Stat, "Statistic").With(definition =>
        {
            definition.Description = "A count of pages, posts, drafts, scheduled posts, items needing attention, or accounts.";
            definition.Icon = "activity";
            definition.W = 2;
            definition.H = 2;
            definition.MinW = 2;
            definition.MinH = 2;
            definition.MaxH = 3;
            definition.Chrome = WidgetChrome.Plain;
            definition.Options =
            [
                WidgetOption.Select(
                    "metric",
                    "Show",
                    DashboardMetrics.PublishedPages,
                    (DashboardMetrics.PublishedPages, "Published pages"),
                    (DashboardMetrics.PublishedPosts, "Published posts"),
                    (DashboardMetrics.Drafts, "Drafts"),
                    (DashboardMetrics.ScheduledPosts, "Scheduled posts"),
                    (DashboardMetrics.Attention, "Needs attention"),
                    (DashboardMetrics.Users, "Accounts")),
            ];
        }),
        WidgetDefinition.For<ActivityWidget>(DashboardKinds.Activity, "Content activity").With(definition =>
        {
            definition.Description = "Pages and posts updated over the last eight weeks.";
            definition.Icon = "chart";
            definition.Color = "blue";
            definition.W = 8;
            definition.H = 5;
            definition.MinW = 4;
            definition.MinH = 4;
        }),
        WidgetDefinition.For<MixWidget>(DashboardKinds.Mix, "Content mix").With(definition =>
        {
            definition.Description = "How pages and posts are split between published, draft and the rest.";
            definition.Icon = "layout";
            definition.Color = "violet";
            definition.W = 4;
            definition.H = 5;
            definition.MinW = 3;
            definition.MinH = 4;
        }),
        WidgetDefinition.For<RecentWidget>(DashboardKinds.Recent, "Recently edited").With(definition =>
        {
            definition.Description = "The pages and posts edited most recently.";
            definition.Icon = "pencil";
            definition.W = 7;
            definition.H = 6;
            definition.MinW = 4;
            definition.MinH = 4;
        }),
        WidgetDefinition.For<AttentionWidget>(DashboardKinds.Attention, "Needs attention").With(definition =>
        {
            definition.Description = "Drafts left untouched, missing search text, switched-off pages and locked accounts.";
            definition.Icon = "alert";
            definition.Color = "orange";
            definition.W = 5;
            definition.H = 6;
            definition.MinW = 3;
            definition.MinH = 4;
        }),
        WidgetDefinition.For<ScheduledWidget>(DashboardKinds.Scheduled, "Scheduled posts").With(definition =>
        {
            definition.Description = "Posts that will be published later.";
            definition.Icon = "clock";
            definition.Color = "teal";
            definition.W = 6;
            definition.H = 4;
            definition.MinW = 3;
            definition.MinH = 3;
        }),
        WidgetDefinition.For<ShortcutsWidget>(DashboardKinds.Shortcuts, "Shortcuts").With(definition =>
        {
            definition.Description = "The admin sections this account can open.";
            definition.Icon = "link";
            definition.W = 6;
            definition.H = 4;
            definition.MinW = 3;
            definition.MinH = 3;
            definition.AutoHeight = true;
        }),
        WidgetDefinition.For<RolesWidget>(DashboardKinds.Roles, "Roles").With(definition =>
        {
            definition.Description = "How many accounts are in each role.";
            definition.Icon = "users";
            definition.Color = "indigo";
            definition.W = 6;
            definition.H = 5;
            definition.MinW = 3;
            definition.MinH = 4;
        }),
        WidgetDefinition.For<AccountsWidget>(DashboardKinds.Accounts, "Accounts").With(definition =>
        {
            definition.Description = "Accounts on this site.";
            definition.Icon = "users";
            definition.W = 6;
            definition.H = 5;
            definition.MinW = 3;
            definition.MinH = 3;
        }),
    ];

    public void AddDefaults(DashboardBoard board)
    {
        board.Sections.Add(new DashboardSection { Id = "overview", Title = "Overview", Icon = "home" });
        board.Sections.Add(new DashboardSection { Id = "content", Title = "Content", Icon = "pencil" });
        board.Sections.Add(new DashboardSection { Id = "people", Title = "People", Icon = "users" });

        board.Items.Add(Stat("published-pages", "Published pages", "code", "green", DashboardMetrics.PublishedPages, 0));
        board.Items.Add(Stat("published-posts", "Published posts", "file", "blue", DashboardMetrics.PublishedPosts, 2));
        board.Items.Add(Stat("drafts", "Drafts", "pencil", "amber", DashboardMetrics.Drafts, 4));
        board.Items.Add(Stat("scheduled", "Scheduled", "clock", "teal", DashboardMetrics.ScheduledPosts, 6));
        board.Items.Add(Stat("attention", "Needs attention", "alert", "orange", DashboardMetrics.Attention, 8));
        board.Items.Add(Stat("accounts", "Accounts", "users", "violet", DashboardMetrics.Users, 10));
        board.Items.Add(Item(DashboardKinds.Activity, "activity", "Content updated", "chart", "blue", "overview", 0, 2, 8, 5));
        board.Items.Add(Item(DashboardKinds.Mix, "mix", "Content mix", "layout", "violet", "overview", 8, 2, 4, 5));

        board.Items.Add(Item(DashboardKinds.Recent, "recent", "Recently edited", "pencil", null, "content", 0, 0, 7, 6));
        board.Items.Add(Item(DashboardKinds.Attention, "needs-attention", "Needs attention", "alert", "orange", "content", 7, 0, 5, 6));
        board.Items.Add(Item(DashboardKinds.Scheduled, "scheduled-list", "Scheduled posts", "clock", "teal", "content", 0, 6, 6, 4));
        board.Items.Add(Item(DashboardKinds.Shortcuts, "shortcuts", "Shortcuts", "link", null, "content", 6, 6, 6, 4));

        board.Items.Add(Item(DashboardKinds.Roles, "roles", "Roles", "users", "indigo", "people", 0, 0, 6, 5));
        board.Items.Add(Item(DashboardKinds.Accounts, "account-list", "Accounts", "users", null, "people", 6, 0, 6, 5));
    }

    private static DashboardItem Stat(string id, string title, string icon, string color, string metric, int x) =>
        Item(DashboardKinds.Stat, id, title, icon, color, "overview", x, 0, 2, 2, WidgetChrome.Plain, new JsonObject { ["metric"] = metric });

    private static DashboardItem Item(
        string kind,
        string id,
        string title,
        string icon,
        string color,
        string section,
        int x,
        int y,
        int w,
        int h,
        WidgetChrome? chrome = null,
        JsonObject options = null) => new()
        {
            Id = id,
            Kind = kind,
            Title = title,
            Icon = icon,
            Color = color,
            Section = section,
            X = x,
            Y = y,
            W = w,
            H = h,
            Chrome = chrome,
            Options = options,
        };
}

internal static class WidgetDefinitionExtensions
{
    public static WidgetDefinition With(this WidgetDefinition definition, Action<WidgetDefinition> configure)
    {
        configure(definition);
        return definition;
    }
}
