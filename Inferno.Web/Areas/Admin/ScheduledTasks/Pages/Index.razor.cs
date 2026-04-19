using Inferno.Web.Areas.Admin.ScheduledTasks.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Inferno.Web.Areas.Admin.ScheduledTasks.Pages;

public partial class Index
{
    [Inject]
    private ScheduledTaskODataService ScheduledTaskODataService { get; set; }

    protected override string GetODataFilter(LoadDataArgs args) => args.Filter;

    protected async Task RunNowAsync(int taskId)
    {
        var response = await ScheduledTaskODataService.RunNowAsync(taskId);
        if (response.Succeeded)
        {
            NotificationService.Notify(
                NotificationSeverity.Success,
                "Success",
                T[InfernoWebLocalizableStrings.ScheduledTasks.ExecutedTaskSuccess]);

            await DataGrid.Reload();
        }
        else
        {
            NotificationService.Notify(
                NotificationSeverity.Error,
                "Error",
                T[InfernoWebLocalizableStrings.ScheduledTasks.ExecutedTaskError]);
        }
    }
}
