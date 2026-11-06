using Ardalis.Result;
using Inferno.Web.Components;
using Inferno.Web.ContentManagement.Areas.Admin.Messaging.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Messaging.Pages;

public partial class MessageTemplates
{
    private IReadOnlyList<MessageTemplateSummary> templates;
    private string search;
    private bool canWrite;

    private MessageTemplateDetails editing;
    private List<TokenDraft> tokenDrafts = [];
    private IReadOnlyDictionary<string, string> errors;
    private bool saving;

    [Inject]
    private IMessageTemplateAdminService TemplateAdminService { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    private bool IsNew => editing?.Id == 0;

    private static string DesignUrl(int id) => $"/admin/messaging/templates/{id}";

    protected override async Task OnInitializedAsync()
    {
        canWrite = await HasPolicyAsync(CmsConstants.Policies.MessagingWrite);
        await LoadAsync();
    }

    private async Task LoadAsync() =>
        templates = await TemplateAdminService.ListAsync(TenantId, search);

    private async Task SearchChangedAsync(string value)
    {
        search = value;
        await LoadAsync();
    }

    private void Create()
    {
        editing = new MessageTemplateDetails();
        tokenDrafts = [];
        errors = null;
    }

    private async Task EditAsync(MessageTemplateSummary template)
    {
        var result = await TemplateAdminService.GetDetailsAsync(TenantId, template.Id);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        editing = result.Value;
        tokenDrafts = editing.Tokens.Select(value => new TokenDraft { Value = value }).ToList();
        errors = null;
    }

    private void AddToken() => tokenDrafts.Add(new TokenDraft());

    private void RemoveToken(TokenDraft row) => tokenDrafts.Remove(row);

    private void Close() => editing = null;

    private async Task SaveAsync()
    {
        if (saving || !await EnsurePolicyAsync(CmsConstants.Policies.MessagingWrite))
        {
            return;
        }

        saving = true;
        try
        {
            bool isNew = IsNew;
            editing.Tokens = tokenDrafts.Select(row => row.Value).ToList();
            var result = await TemplateAdminService.SaveDetailsAsync(TenantId, editing);
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

            editing = null;
            if (isNew)
            {
                NavigationManager.NavigateTo(DesignUrl(result.Value.Id));
                return;
            }

            await LoadAsync();
            Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Saved]);
        }
        finally
        {
            saving = false;
        }
    }

    private async Task DeleteAsync(MessageTemplateSummary template)
    {
        bool confirmed = await Dialogs.ConfirmAsync(
            T[InfernoWebLocalizableStrings.Admin.DeleteTitleFormat, template.Name],
            T[InfernoCmsLocalizableStrings.Messaging.DeleteWarning],
            T[InfernoWebLocalizableStrings.General.Delete],
            isDestructive: true);

        if (!confirmed || !await EnsurePolicyAsync(CmsConstants.Policies.MessagingWrite))
        {
            return;
        }

        var result = await TemplateAdminService.DeleteAsync(TenantId, template.Id);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        await LoadAsync();
        Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Deleted]);
    }

    private sealed class TokenDraft
    {
        /// <summary>
        /// Only used as a unique key for the row in the table. It is not persisted to the database and is not used in the template.
        /// </summary>
        public string Key { get; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// The token value, e.g. <c>{{ FirstName }}</c>. It is persisted to the database and used in the template.
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }
}