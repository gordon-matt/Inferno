using Microsoft.AspNetCore.Components;

namespace InfernoCMS.Shared
{
    public partial class NavMenu
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private void NavigateToAdminArea()
        {
            NavigationManager.NavigateTo("/admin/", forceLoad: true);
        }
    }
}