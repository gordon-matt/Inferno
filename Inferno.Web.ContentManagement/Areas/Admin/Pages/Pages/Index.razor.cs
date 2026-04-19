using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Pages;

public partial class Index
{
    [Inject]
    private IPageTypeService PageTypeService { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    private void NavigateToVersions(Guid pageId) =>
        NavigationManager.NavigateTo($"/admin/pages/versions/{pageId}");

    private IEnumerable<PageType> PageTypesSelectList { get; set; } = Enumerable.Empty<PageType>();

    private bool ShowCreateMode { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        LoadPageTypes();
    }

    protected override void Create()
    {
        Model = new Page
        {
            IsEnabled = true,
            ShowOnMenus = true,
            PageTypeId = PageTypesSelectList.FirstOrDefault()?.Id ?? Guid.Empty
        };
        ShowEditMode = false;
        ShowCreateMode = true;
    }

    protected override async Task EditAsync(Guid id)
    {
        ShowCreateMode = false;
        await base.EditAsync(id);
    }

    protected override async Task OnValidSumbitAsync()
    {
        await base.OnValidSumbitAsync();
        ShowCreateMode = false;
    }

    protected override void Cancel()
    {
        base.Cancel();
        ShowCreateMode = false;
    }

    private void LoadPageTypes()
    {
        using var connection = PageTypeService.OpenConnection();
        PageTypesSelectList = connection.Query()
            .OrderBy(x => x.Name)
            .ToList();
    }

    private string GetPageTypeName(Guid pageTypeId) =>
        PageTypesSelectList.FirstOrDefault(x => x.Id == pageTypeId)?.Name ?? string.Empty;
}
