using System.Text.Json.Nodes;
using GridKrafter.Components;
using GridKrafter.Models;
using Inferno.Plugins.GoogleAnalytics.Dashboard;
using Inferno.Web.Dashboard;

namespace Inferno.Plugins.GoogleAnalytics;

public sealed class GoogleAnalyticsDashboardContributor : IDashboardContributor
{
    public int Order => 100;

    public IReadOnlyList<WidgetDefinition> Widgets { get; } =
    [
        WidgetDefinition.For<VisitorWidget>("ga.visitors", "Visitors").With(definition =>
        {
            definition.Description = "Active users and page views for today, the last 7 days and the last 28 days.";
            definition.Icon = "users";
            definition.Color = "sky";
            definition.W = 4;
            definition.H = 3;
            definition.MinW = 3;
            definition.MinH = 2;
            definition.Single = true;
        }),
        WidgetDefinition.For<TrafficWidget>("ga.traffic", "Traffic").With(definition =>
        {
            definition.Description = "Visitors and page views over the last 7, 28 or 90 days.";
            definition.Icon = "chart";
            definition.Color = "blue";
            definition.W = 8;
            definition.H = 5;
            definition.MinW = 4;
            definition.MinH = 4;
            definition.Single = true;
            definition.Options =
            [
                WidgetOption.Select("days", "Range", "28", ("7", "7 days"), ("28", "28 days"), ("90", "90 days")),
            ];
        }),
        WidgetDefinition.For<TopPagesWidget>("ga.top-pages", "Top pages").With(definition =>
        {
            definition.Description = "The most viewed pages in the last 28 days.";
            definition.Icon = "file";
            definition.W = 6;
            definition.H = 5;
            definition.MinW = 3;
            definition.MinH = 3;
            definition.Single = true;
        }),
        Breakdown("ga.sources", "Traffic sources", "Where visits came from in the last 28 days.", "globe", "sources"),
        Breakdown("ga.devices", "Devices", "Desktop, mobile and tablet over the last 28 days.", "monitor", "devices"),
    ];

    public void AddDefaults(DashboardBoard board)
    {
        board.Sections.Add(new DashboardSection { Id = "analytics", Title = "Analytics", Icon = "chart" });
        board.Items.Add(Item("ga.visitors", "ga-visitors", "Visitors", "users", "sky", 0, 0, 4, 3));
        board.Items.Add(Item("ga.traffic", "ga-traffic", "Traffic", "chart", "blue", 4, 0, 8, 5, new JsonObject { ["days"] = "28" }));
        board.Items.Add(Item("ga.top-pages", "ga-pages", "Top pages", "file", null, 0, 5, 6, 5));
        board.Items.Add(Item("ga.sources", "ga-sources", "Traffic sources", "globe", "teal", 6, 5, 3, 5, new JsonObject { ["series"] = "sources" }));
        board.Items.Add(Item("ga.devices", "ga-devices", "Devices", "monitor", "indigo", 9, 5, 3, 5, new JsonObject { ["series"] = "devices" }));
    }

    private static WidgetDefinition Breakdown(string kind, string name, string description, string icon, string series) =>
        WidgetDefinition.For<RankWidget>(kind, name).With(definition =>
        {
            definition.Description = description;
            definition.Icon = icon;
            definition.W = 3;
            definition.H = 5;
            definition.MinW = 2;
            definition.MinH = 3;
            definition.Single = true;
            definition.Options =
            [
                WidgetOption.Select("series", "Show", series, ("sources", "Traffic sources"), ("devices", "Devices")),
            ];
        });

    private static DashboardItem Item(string kind, string id, string title, string icon, string? color, int x, int y, int w, int h, JsonObject? options = null) =>
        new()
        {
            Id = id,
            Kind = kind,
            Section = "analytics",
            Title = title,
            Icon = icon,
            Color = color,
            X = x,
            Y = y,
            W = w,
            H = h,
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
