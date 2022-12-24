using Inferno.Web.Areas.Admin.Localization.Models;
using Inferno.Web.Areas.Admin.Localization.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Radzen;
using Radzen.Blazor;

namespace Inferno.Web.Areas.Admin.Localization.Pages
{
    public partial class LocalizableStrings
    {
        private ComparitiveLocalizableString editRow;

        [Parameter]
        public string CultureCode { get; set; }

        private RadzenDataGrid<ComparitiveLocalizableString> DataGrid { get; set; }

        private IEnumerable<ComparitiveLocalizableString> Records { get; set; }

        private bool IsLoading { get; set; }

        private int RecordCount { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private DialogService DialogService { get; set; }

        [Inject]
        private NotificationService NotificationService { get; set; }

        [Inject]
        private LocalizableStringODataService ODataService { get; set; }

        private async Task LoadGridAsync(LoadDataArgs args)
        {
            IsLoading = true;
            var result = await ODataService.GetComparitiveAsync(CultureCode, args);
            Records = result.Value.AsODataEnumerable();
            RecordCount = result.Count;
            IsLoading = false;
        }

        private async Task EditAsync(ComparitiveLocalizableString row)
        {
            editRow = row;
            await DataGrid.EditRow(row);
        }

        private async Task DeleteAsync(ComparitiveLocalizableString row)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    if (row == editRow)
                    {
                        editRow = null;
                    }

                    bool success = await ODataService.DeleteComparitiveAsync(CultureCode, row.Key);
                    if (!success)
                    {
                        NotificationService.Notify(NotificationSeverity.Error, "Error", "Unable to delete record!");
                    }

                    await DataGrid.Reload();
                    NotificationService.Notify(NotificationSeverity.Info, "Info", "Record deleted!");
                }
            }
            catch
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Unable to delete record!");
                // TODO: Log Error
            }
        }

        private async Task SaveAsync(ComparitiveLocalizableString row)
        {
            try
            {
                var result = await ODataService.PutComparitiveAsync(CultureCode, row);
                NotificationService.Notify(NotificationSeverity.Info, "Info", "Record updated!");
                await DataGrid.Reload();
            }
            catch
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Unable to save record!");
                // TODO: Log Error
            }
        }

        private async Task CancelAsync(ComparitiveLocalizableString row)
        {
            editRow = null;
            DataGrid.CancelEditRow(row);
        }

        private void ExportLanguagePack()
        {
            NavigationManager.NavigateTo($"/admin/localization/localizable-strings/export/{CultureCode}", forceLoad: true);
        }
    }
}