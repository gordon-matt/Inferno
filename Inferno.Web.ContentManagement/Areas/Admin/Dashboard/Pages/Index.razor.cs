using GridKrafter.Components;
using GridKrafter.Models;
using Inferno.Web.Components;
using Inferno.Web.Dashboard;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Dashboard.Pages;

public partial class Index : AdminPage, IDisposable
{
    [Inject]
    private IEnumerable<IDashboardContributor> Contributors { get; set; }

    [Inject]
    private IDashboardBoardStore Boards { get; set; }

    private IReadOnlyList<WidgetDefinition> definitions = [];
    private DashboardBoard board;
    private DashboardBoard saved;
    private CancellationTokenSource pendingSave;
    private string userId;

    protected override void OnInitialized()
    {
        var contributors = Contributors.OrderBy(contributor => contributor.Order).ToList();
        definitions = [.. contributors.SelectMany(contributor => contributor.Widgets)];
        board = new DashboardBoard();
        foreach (var contributor in contributors)
        {
            contributor.AddDefaults(board);
        }

        board.Tidy();
        userId = WorkContext.CurrentUser?.Id;
        if (!string.IsNullOrEmpty(userId))
        {
            saved = Boards.Load(TenantId, userId);
        }
    }

    private async Task SaveAsync(DashboardBoard changed)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        pendingSave?.Cancel();
        pendingSave?.Dispose();
        pendingSave = new CancellationTokenSource();
        var token = pendingSave.Token;
        try
        {
            await Task.Delay(500, token);
            Boards.Save(TenantId, userId, changed);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose()
    {
        pendingSave?.Cancel();
        pendingSave?.Dispose();
    }
}
