using Ardalis.Result;
using Heroicsoft.NetForge.Results;
using Inferno.Web.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;

/// <summary>Lists the tenant's zones in a drawer, with add, rename and delete.</summary>
public partial class ZoneManager
{
    private sealed class ZoneForm
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }

    private IReadOnlyList<ZoneSummary> zones;
    private ZoneForm form = new();
    private IReadOnlyDictionary<string, string> errors;
    private bool canWrite;
    private bool saving;
    private bool wasOpen;

    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>Raised after a zone was added, renamed or removed, so the page can refresh its zone lists.</summary>
    [Parameter]
    public EventCallback OnChanged { get; set; }

    [Inject]
    private IContentBlockAdminService ContentBlockAdminService { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (!Open)
        {
            wasOpen = false;
            return;
        }

        if (wasOpen)
        {
            return;
        }

        wasOpen = true;
        canWrite = await HasPolicyAsync(CmsConstants.Policies.ContentZonesWrite);
        form = new ZoneForm();
        errors = null;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var result = await ContentBlockAdminService.ListZoneSummariesAsync(TenantId);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            zones = [];
            return;
        }

        zones = result.Value;
    }

    private void BeginEdit(ZoneSummary zone)
    {
        errors = null;
        form = new ZoneForm { Id = zone.Id, Name = zone.Name };
    }

    private void CancelEdit()
    {
        errors = null;
        form = new ZoneForm();
    }

    private Task CloseAsync() => OnClose.InvokeAsync();

    private async Task SaveAsync()
    {
        if (saving || !await EnsurePolicyAsync(CmsConstants.Policies.ContentZonesWrite))
        {
            return;
        }

        saving = true;
        try
        {
            var result = await ContentBlockAdminService.SaveZoneAsync(TenantId, form.Id, form.Name);
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
            CancelEdit();
            await LoadAsync();
            await OnChanged.InvokeAsync();
        }
        finally
        {
            saving = false;
        }
    }

    private async Task DeleteAsync(ZoneSummary zone)
    {
        if (zone.BlockCount > 0)
        {
            // Refused by the service as well; saying so up front saves a confirmation that cannot succeed.
            Toasts.Error(T[InfernoCmsLocalizableStrings.ContentBlocks.ZoneInUseFormat, zone.Name, zone.BlockCount]);
            return;
        }

        bool confirmed = await Dialogs.ConfirmAsync(
            T[InfernoWebLocalizableStrings.Admin.DeleteTitleFormat, zone.Name],
            T[InfernoWebLocalizableStrings.Admin.DeleteWarning],
            T[InfernoWebLocalizableStrings.General.Delete],
            isDestructive: true);

        if (!confirmed || !await EnsurePolicyAsync(CmsConstants.Policies.ContentZonesWrite))
        {
            return;
        }

        var result = await ContentBlockAdminService.DeleteZoneAsync(TenantId, zone.Id);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Deleted]);
        if (form.Id == zone.Id)
        {
            CancelEdit();
        }

        await LoadAsync();
        await OnChanged.InvokeAsync();
    }
}
