using GridKrafter.Components;
using Microsoft.AspNetCore.Components;

namespace Inferno.Plugins.GoogleAnalytics.Dashboard;

/// <summary>Loads an <see cref="AnalyticsBundle"/> when the widget is created, and again when the dashboard asks it to refresh.</summary>
public abstract class AnalyticsWidget : WidgetComponentBase
{
    /// <summary>The report service.</summary>
    [Inject]
    protected IAnalyticsQueryService Analytics { get; set; } = default!;

    /// <summary>The last report. <see langword="null"/> until the first load finishes.</summary>
    protected AnalyticsBundle? Data { get; private set; }

    protected override async Task OnInitializedAsync() => Data = await Analytics.GetAsync(false);

    /// <summary>Reloads the report, bypassing the cache.</summary>
    /// <returns>A task that completes when <see cref="Data"/> has been replaced.</returns>
    protected override async Task OnRefreshAsync() => Data = await Analytics.GetAsync(true);
}
