using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Menus.Pages;

public partial class Index
{
    [Inject]
    private NavigationManager NavigationManager { get; set; }

    private void NavigateToItems(Guid menuId) => NavigationManager.NavigateTo($"/admin/menus/items/{menuId}");
}
