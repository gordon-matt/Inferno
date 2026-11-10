using System.Globalization;
using GridKrafter.Components;
using Inferno.Web.ContentManagement.Areas.Admin.Dashboard;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Dashboard.Widgets;

public abstract class InfernoWidget : WidgetComponentBase
{
    [Inject]
    protected DashboardSnapshotSource Snapshots { get; set; }

    protected DashboardSnapshot Snapshot { get; private set; }

    protected override async Task OnInitializedAsync() => Snapshot = await Snapshots.GetAsync(false);

    protected override async Task OnRefreshAsync() => Snapshot = await Snapshots.GetAsync(true);

    protected string When(DateTime utc)
    {
        var local = Context is null ? utc : utc.Add(Context.ClientOffset);
        return local.ToString("d MMM yyyy, HH:mm", CultureInfo.CurrentCulture);
    }
}
