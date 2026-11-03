using Heroicsoft.NetForge.Results;
using Inferno.Web.ContentManagement.Areas.Admin.Messaging.Services;
using Inferno.Web.Models;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Messaging.Pages;

public partial class QueuedEmails
{
    private ListPage<QueuedEmailSummary> listing;
    private string search;
    private QueuedEmailStatus status = QueuedEmailStatus.All;
    private int pageNumber = 1;
    private int pageSize = 24;
    private bool canWrite;

    private QueuedEmailMessage viewing;

    [Inject]
    private IQueuedEmailAdminService QueuedEmailAdminService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        canWrite = await HasPolicyAsync(CmsConstants.Policies.MessagingWrite);
        await LoadAsync();
    }

    private static string FormatAddress(string address, string name) =>
        string.IsNullOrWhiteSpace(name) ? address : $"{name} <{address}>";

    private async Task LoadAsync()
    {
        var result = await QueuedEmailAdminService.SearchAsync(TenantId, new ListQuery(search, PageNumber: pageNumber, PageSize: pageSize), status);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            listing = ListPage<QueuedEmailSummary>.Empty;
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

    private async Task StatusChangedAsync(ChangeEventArgs args)
    {
        status = Enum.TryParse<QueuedEmailStatus>(args.Value?.ToString(), out var parsed) ? parsed : QueuedEmailStatus.All;
        pageNumber = 1;
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

    private async Task OpenAsync(QueuedEmailSummary email)
    {
        var result = await QueuedEmailAdminService.GetAsync(TenantId, email.Id);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        viewing = result.Value;
    }

    private void Close() => viewing = null;

    private async Task DeleteAsync(QueuedEmailSummary email)
    {
        bool confirmed = await Dialogs.ConfirmAsync(
            T[InfernoWebLocalizableStrings.Admin.DeleteTitleFormat, email.Subject],
            T[InfernoCmsLocalizableStrings.Messaging.DeleteEmailWarning],
            T[InfernoWebLocalizableStrings.General.Delete],
            isDestructive: true);

        if (!confirmed || !await EnsurePolicyAsync(CmsConstants.Policies.MessagingWrite))
        {
            return;
        }

        var result = await QueuedEmailAdminService.DeleteAsync(TenantId, email.Id);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        viewing = null;
        await LoadAsync();
        Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Deleted]);
    }
}
