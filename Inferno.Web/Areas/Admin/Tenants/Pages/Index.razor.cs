using Ardalis.Result;
using Heroicsoft.NetForge.Components.Tables;
using Heroicsoft.NetForge.Results;
using Inferno.Security;
using Inferno.Tenants.Entities;
using Inferno.Web.Areas.Admin.Tenants.Services;
using Inferno.Web.Components;
using Inferno.Web.Models;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Areas.Admin.Tenants.Pages;

public partial class Index
{
    private ListPage<Tenant> listing;
    private string search;
    private TableSort sort = new(nameof(Tenant.Name));
    private int pageNumber = 1;
    private int pageSize = 24;

    private Tenant editing;
    private IReadOnlyDictionary<string, string> errors;
    private bool saving;

    [Inject]
    private ITenantAdminService TenantAdminService { get; set; }

    protected override Task OnInitializedAsync() => LoadAsync();

    private string ExportUrl(string format) =>
        string.IsNullOrWhiteSpace(search)
            ? $"/admin/tenants/export/{format}"
            : $"/admin/tenants/export/{format}?search={Uri.EscapeDataString(search)}";

    private async Task LoadAsync()
    {
        var result = await TenantAdminService.SearchAsync(new ListQuery(search, sort.Field, sort.Descending, pageNumber, pageSize));
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            listing = ListPage<Tenant>.Empty;
            return;
        }

        listing = result.Value;
    }

    private async Task SearchChangedAsync(string value)
    {
        search = value;
        pageNumber = 1;
        await LoadAsync();
    }

    private async Task SortChangedAsync(TableSort value)
    {
        sort = value;
        await LoadAsync();
    }

    private async Task PageChangedAsync(int value)
    {
        pageNumber = value;
        await LoadAsync();
    }

    private async Task PageSizeChangedAsync(int value)
    {
        pageSize = value;
        pageNumber = 1;
        await LoadAsync();
    }

    private void Create() => Open(new Tenant());

    // Edit a copy, so cancelling leaves the row as it was.
    private void Edit(Tenant tenant) => Open(new Tenant
    {
        Id = tenant.Id,
        Name = tenant.Name,
        Url = tenant.Url,
        Hosts = tenant.Hosts,
        SslEnabled = tenant.SslEnabled,
        SecureUrl = tenant.SecureUrl,
    });

    private void Open(Tenant tenant)
    {
        errors = null;
        editing = tenant;
    }

    private void Close() => editing = null;

    private async Task SaveAsync()
    {
        if (saving || !await EnsurePolicyAsync(StandardPolicies.FullAccess))
        {
            return;
        }

        saving = true;
        try
        {
            var result = await TenantAdminService.SaveAsync(editing);
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
            await LoadAsync();
        }
        finally
        {
            saving = false;
        }
    }

    private async Task DeleteAsync(Tenant tenant)
    {
        bool confirmed = await Dialogs.ConfirmAsync(
            T[InfernoWebLocalizableStrings.Admin.DeleteTitleFormat, tenant.Name],
            T[InfernoWebLocalizableStrings.Tenants.DeleteWarning],
            T[InfernoWebLocalizableStrings.General.Delete],
            isDestructive: true);

        if (!confirmed || !await EnsurePolicyAsync(StandardPolicies.FullAccess))
        {
            return;
        }

        var result = await TenantAdminService.DeleteAsync(tenant.Id);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Deleted]);
        await LoadAsync();
    }
}
