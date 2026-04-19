using Extenso;
using Inferno.Web.Areas.Admin.Plugins.Models;
using Inferno.Web.Areas.Admin.Plugins.Services;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace Inferno.Web.Areas.Admin.Plugins.Pages
{
    public partial class Index : ComponentBase
    {
        protected RadzenDataGrid<EdmPluginDescriptor> DataGrid { get; set; }

        protected IEnumerable<EdmPluginDescriptor> Records { get; set; }

        protected int RecordCount { get; set; }

        protected bool IsLoading { get; set; }

        protected bool ShowEditMode { get; set; }

        protected EdmPluginDescriptor Model { get; set; } = new();

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        protected PluginODataService ODataService { get; set; }

        protected virtual async Task LoadGridAsync(LoadDataArgs args)
        {
            IsLoading = true;

            var response = await ODataService.FindAsync(
                filter: args.Filter,
                top: args.Top,
                skip: args.Skip,
                orderby: args.OrderBy,
                count: true);

            if (response.Succeeded)
            {
                Records = response.Data.Value.AsODataEnumerable();
                RecordCount = response.Data.Count;
            }
            else
            {
                Records = Enumerable.Empty<EdmPluginDescriptor>();
                RecordCount = 0;
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Unable to retrieve plugins!");
            }

            IsLoading = false;
        }

        protected virtual async Task EditAsync(EdmPluginDescriptor descriptor)
        {
            var response = await ODataService.FindOneAsync(descriptor.Id);
            if (response.Succeeded)
            {
                Model = response.Data;
                ShowEditMode = true;
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Unable to retrieve plugin!");
            }
        }

        protected virtual async Task OnValidSubmitAsync()
        {
            var response = await ODataService.UpdateAsync(Model.Id, Model);
            if (response.Succeeded)
            {
                ShowEditMode = false;
                NotificationService.Notify(NotificationSeverity.Info, "Info", "Plugin updated!");
                await DataGrid.Reload();
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Unable to update plugin!");
            }
        }

        protected virtual void Cancel()
        {
            Model = new EdmPluginDescriptor();
            ShowEditMode = false;
        }

        protected virtual async Task InstallAsync(EdmPluginDescriptor descriptor)
        {
            if (await DialogService.Confirm(
                $"Install plugin '{descriptor.FriendlyName}'?",
                "Confirm",
                new ConfirmOptions { OkButtonText = "Install", CancelButtonText = "Cancel" }) != true)
            {
                return;
            }

            var response = await ODataService.InstallAsync(descriptor.SystemName);
            if (response.Succeeded)
            {
                NotificationService.Notify(
                    NotificationSeverity.Success,
                    "Success",
                    T[InfernoWebLocalizableStrings.Plugins.InstallPluginSuccess]);
                await DataGrid.Reload();
            }
            else
            {
                NotificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    T[InfernoWebLocalizableStrings.Plugins.InstallPluginError]);
            }
        }

        protected virtual async Task UninstallAsync(EdmPluginDescriptor descriptor)
        {
            if (await DialogService.Confirm(
                $"Uninstall plugin '{descriptor.FriendlyName}'?",
                "Confirm",
                new ConfirmOptions { OkButtonText = "Uninstall", CancelButtonText = "Cancel" }) != true)
            {
                return;
            }

            var response = await ODataService.UninstallAsync(descriptor.SystemName);
            if (response.Succeeded)
            {
                NotificationService.Notify(
                    NotificationSeverity.Success,
                    "Success",
                    T[InfernoWebLocalizableStrings.Plugins.UninstallPluginSuccess]);
                await DataGrid.Reload();
            }
            else
            {
                NotificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    T[InfernoWebLocalizableStrings.Plugins.UninstallPluginError]);
            }
        }
    }
}
