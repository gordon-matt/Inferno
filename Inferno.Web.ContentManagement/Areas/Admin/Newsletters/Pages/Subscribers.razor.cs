using Heroicsoft.NetForge.Results;
using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Models;
using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Pages;

public partial class Subscribers
{
    private IReadOnlyList<Subscriber> subscribers;
    private string search;
    private bool canWrite;

    [Inject]
    private ISubscriberAdminService SubscriberAdminService { get; set; }

    private string ExportUrl =>
        string.IsNullOrWhiteSpace(search)
            ? "/admin/newsletters/subscribers/export/csv"
            : $"/admin/newsletters/subscribers/export/csv?search={Uri.EscapeDataString(search)}";

    protected override async Task OnInitializedAsync()
    {
        canWrite = await HasPolicyAsync(CmsConstants.Policies.NewsletterWrite);
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var result = await SubscriberAdminService.ListAsync(TenantId, search);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            subscribers = [];
            return;
        }

        subscribers = result.Value;
    }

    private async Task SearchChangedAsync(string value)
    {
        search = value;
        await LoadAsync();
    }

    private async Task UnsubscribeAsync(Subscriber subscriber)
    {
        bool confirmed = await Dialogs.ConfirmAsync(
            T[InfernoCmsLocalizableStrings.Newsletters.UnsubscribeTitleFormat, subscriber.Name ?? subscriber.Email],
            T[InfernoCmsLocalizableStrings.Newsletters.UnsubscribeWarning],
            T[InfernoCmsLocalizableStrings.Newsletters.Unsubscribe],
            isDestructive: true);

        if (!confirmed || !await EnsurePolicyAsync(CmsConstants.Policies.NewsletterWrite))
        {
            return;
        }

        var result = await SubscriberAdminService.UnsubscribeAsync(TenantId, subscriber.Id);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        Toasts.Success(T[InfernoCmsLocalizableStrings.Newsletters.Unsubscribed]);
        await LoadAsync();
    }
}
