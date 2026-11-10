using GridKrafter.Components;
using Microsoft.AspNetCore.Components;

namespace Inferno.Plugins.GoogleAnalytics.Dashboard;

public abstract class AnalyticsWidget : WidgetComponentBase
{
    [Inject]
    protected IAnalyticsQueryService Analytics { get; set; } = default!;

    protected AnalyticsBundle? Data { get; private set; }

    protected override async Task OnInitializedAsync() => Data = await Analytics.GetAsync(false);

    protected override async Task OnRefreshAsync() => Data = await Analytics.GetAsync(true);
}
