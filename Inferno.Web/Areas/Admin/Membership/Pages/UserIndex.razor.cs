using Microsoft.AspNetCore.Components;
using Radzen;

namespace Inferno.Web.Areas.Admin.Membership.Pages
{
    public partial class UserIndex
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; }

        protected override string GetODataFilter(LoadDataArgs args) => args.Filter;

        public async Task SearchAsync()
        {
            DataGrid.Reset();
            await DataGrid.Reload();
        }

        public void Export(string type)
        {
            var query = new Query { OrderBy = DataGrid.Query.OrderBy, Filter = DataGrid.Query.Filter };
            NavigationManager.NavigateTo(query.ToUrl($"/admin/membership/users/export/{type}"), forceLoad: true);
        }

        public void Roles()
        {
            NavigationManager.NavigateTo("/admin/membership/roles/index", forceLoad: true);
        }
    }
}