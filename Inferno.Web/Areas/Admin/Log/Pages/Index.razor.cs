using Inferno.Web.Areas.Admin.Log.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Inferno.Web.Areas.Admin.Log.Pages;

public partial class Index
{
    [Inject]
    private LogODataService LogODataService { get; set; }

    protected override string GetODataFilter(LoadDataArgs args) => args.Filter;

    protected async Task ClearAsync()
    {
        try
        {
            if (await DialogService.Confirm(T[InfernoWebLocalizableStrings.Log.ClearConfirm]) != true)
            {
                return;
            }

            var response = await LogODataService.ClearAsync();
            if (response.Succeeded)
            {
                NotificationService.Notify(
                    NotificationSeverity.Success,
                    "Success",
                    T[InfernoWebLocalizableStrings.Log.ClearSuccess]);

                await DataGrid.Reload();
            }
            else
            {
                NotificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    T[InfernoWebLocalizableStrings.Log.ClearError]);
            }
        }
        catch
        {
            NotificationService.Notify(
                NotificationSeverity.Error,
                "Error",
                T[InfernoWebLocalizableStrings.Log.ClearError]);
        }
    }
}
