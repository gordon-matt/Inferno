using Extenso;
using Extenso.Data.Entity;
using Inferno.Web.OData;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace Inferno.Web.Components
{
    public abstract class RadzenDataPage<TEntity, TKey> : ComponentBase
        where TEntity : class, IEntity, new()
    {
        protected TEntity Model { get; set; } = new();

        protected RadzenDataGrid<TEntity> DataGrid { get; set; }

        protected IEnumerable<TEntity> Records { get; set; }

        protected int RecordCount { get; set; }

        protected bool IsLoading { get; set; }

        protected bool ShowEditMode { get; set; }

        #region Dependencies

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        protected IRadzenODataService<TEntity, TKey> ODataService { get; set; }

        #endregion Dependencies

        #region CRUD Operations

        protected virtual async Task LoadGridAsync(LoadDataArgs args)
        {
            IsLoading = true;
            string odataFilter = GetODataFilter(args);
            var result = await ODataService.FindAsync(filter: odataFilter, top: args.Top, skip: args.Skip, orderby: args.OrderBy, count: true);
            Records = result.Value.AsODataEnumerable();
            RecordCount = result.Count;
            IsLoading = false;
        }

        protected virtual string GetODataFilter(LoadDataArgs args) => args.Filter;

        protected virtual void Create()
        {
            Model = new TEntity();
            ShowEditMode = true;
        }

        protected virtual async Task EditAsync(TKey id)
        {
            Model = await ODataService.FindOneAsync(id);
            ShowEditMode = true;
        }

        protected virtual async Task OnValidSumbitAsync()
        {
            try
            {
                var key = (TKey)Model.KeyValues.First();
                if (key.IsDefault())
                {
                    var result = await ODataService.InsertAsync(Model);
                    NotificationService.Notify(NotificationSeverity.Info, "Info", "Record created!");
                }
                else
                {
                    var result = await ODataService.UpdateAsync(key, Model);
                    NotificationService.Notify(NotificationSeverity.Info, "Info", "Record updated!");
                }

                await DataGrid.Reload();
                ShowEditMode = false;
            }
            catch
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Unable to save record!");
                // TODO: Log Error
            }
        }

        protected virtual async Task DeleteAsync(TKey id)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    await ODataService.DeleteAsync(id);
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

        protected virtual void Cancel()
        {
            Model = new TEntity();
            ShowEditMode = false;
        }

        #endregion CRUD Operations
    }
}