using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Inferno.Web.ContentManagement.Areas.Admin.Menus.Pages
{
    public partial class Items
    {
        [Parameter]
        public Guid MenuId { get; set; }

        [Inject]
        private IMenuService MenuService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private string MenuName { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            LoadMenuName();
        }

        protected override Task LoadGridAsync(LoadDataArgs args) => base.LoadGridAsync(args);

        protected override string GetODataFilter(LoadDataArgs args)
        {
            string menuFilter = $"MenuId eq {MenuId}";
            return string.IsNullOrWhiteSpace(args.Filter)
                ? menuFilter
                : $"({args.Filter}) and {menuFilter}";
        }

        protected override void Create()
        {
            Model = new MenuItem
            {
                MenuId = MenuId,
                Enabled = true,
                Position = 0
            };
            ShowEditMode = true;
        }

        private void LoadMenuName()
        {
            using var connection = MenuService.OpenConnection();
            var menu = connection.Query(x => x.Id == MenuId).FirstOrDefault();
            MenuName = menu?.Name ?? string.Empty;
        }

        private void GoBack() => NavigationManager.NavigateTo("/admin/menus/index");
    }
}
