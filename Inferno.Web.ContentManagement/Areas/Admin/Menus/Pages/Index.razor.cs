using Ardalis.Result;
using Heroicsoft.NetForge.Results;
using Inferno.Web.Components;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Menus.Pages;

public partial class Index
{
    private IReadOnlyList<MenuSummary> menus;
    private string search;
    private bool canWrite;

    private Menu editing;
    private IReadOnlyDictionary<string, string> errors;
    private bool saving;

    [Inject]
    private IMenuAdminService MenuAdminService { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    protected override async Task OnInitializedAsync()
    {
        canWrite = await HasPolicyAsync(CmsConstants.Policies.MenusWrite);
        await LoadAsync();
    }

    private static string ItemsUrl(Guid menuId) => $"/admin/menus/items/{menuId}";

    private async Task LoadAsync()
    {
        var result = await MenuAdminService.ListMenusAsync(TenantId, search);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            menus = [];
            return;
        }

        menus = result.Value;
    }

    private async Task SearchChangedAsync(string value)
    {
        search = value;
        await LoadAsync();
    }

    private void Create() => Open(new Menu());

    private void Edit(MenuSummary menu) => Open(new Menu { Id = menu.Id, Name = menu.Name, UrlFilter = menu.UrlFilter });

    private void Open(Menu menu)
    {
        errors = null;
        editing = menu;
    }

    private void Close() => editing = null;

    private async Task SaveAsync()
    {
        if (saving || !await EnsurePolicyAsync(CmsConstants.Policies.MenusWrite))
        {
            return;
        }

        saving = true;
        try
        {
            bool isNew = editing.Id == Guid.Empty;
            var result = await MenuAdminService.SaveMenuAsync(TenantId, editing);
            if (result.Status == ResultStatus.Invalid)
            {
                errors = ErrorText.From(result);
                return;
            }

            if (!result.IsSuccess)
            {
                Toasts.ShowFailure(result);
                return;
            }

            Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Saved]);
            editing = null;

            if (isNew)
            {
                NavigationManager.NavigateTo(ItemsUrl(result.Value.Id));
                return;
            }

            await LoadAsync();
        }
        finally
        {
            saving = false;
        }
    }

    private async Task DeleteAsync(MenuSummary menu)
    {
        bool confirmed = await Dialogs.ConfirmAsync(
            T[InfernoWebLocalizableStrings.Admin.DeleteTitleFormat, menu.Name],
            T[InfernoCmsLocalizableStrings.Menus.DeleteWarning],
            T[InfernoWebLocalizableStrings.General.Delete],
            isDestructive: true);

        if (!confirmed || !await EnsurePolicyAsync(CmsConstants.Policies.MenusWrite))
        {
            return;
        }

        var result = await MenuAdminService.DeleteMenuAsync(TenantId, menu.Id);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Deleted]);
        await LoadAsync();
    }
}
