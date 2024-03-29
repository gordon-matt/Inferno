﻿using Microsoft.AspNetCore.Components;
using Radzen;

namespace Inferno.Web.Areas.Admin.Tenants.Pages
{
    public partial class Index
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; }

        protected override string GetODataFilter(LoadDataArgs args)
        {
            return args.Filter;
        }

        public async Task SearchAsync()
        {
            DataGrid.Reset();
            await DataGrid.Reload();
        }

        public void Export(string type)
        {
            var query = new Query { OrderBy = DataGrid.Query.OrderBy, Filter = DataGrid.Query.Filter };
            NavigationManager.NavigateTo(query.ToUrl($"/admin/tenants/export/{type}"), forceLoad: true);
        }
    }
}