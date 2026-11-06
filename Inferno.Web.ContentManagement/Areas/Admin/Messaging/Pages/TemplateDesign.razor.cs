using Heroicsoft.NetForge.Results;
using Inferno.Web.ContentManagement.Areas.Admin.Messaging.Services;
using Microsoft.AspNetCore.Components;
using PageKrafter.Documents;
using PageKrafter.Editing;

namespace Inferno.Web.ContentManagement.Areas.Admin.Messaging.Pages;

public partial class TemplateDesign
{
    private MessageTemplateDocument template;
    private string body;
    private IReadOnlyList<EditorToken> tokens = [];
    private string message;
    private string savedAt;
    private bool canWrite;

    [Parameter]
    public int TemplateId { get; set; }

    [Inject]
    private IMessageTemplateAdminService TemplateAdminService { get; set; }

    protected override async Task OnInitializedAsync() =>
        canWrite = await HasPolicyAsync(CmsConstants.Policies.MessagingWrite);

    protected override async Task OnParametersSetAsync()
    {
        if (template?.Id == TemplateId)
        {
            return;
        }

        var result = await TemplateAdminService.GetDocumentAsync(TenantId, TemplateId);
        if (!result.IsSuccess)
        {
            template = null;
            message = T[InfernoCmsLocalizableStrings.Messaging.TemplateNotFound].Value;
            return;
        }

        template = result.Value;
        body = template.Body;
        tokens = template.Tokens.Select(value => new EditorToken(value)).ToArray();
        savedAt = null;
    }

    private async Task SaveAsync(PageDocument document)
    {
        if (!await EnsurePolicyAsync(CmsConstants.Policies.MessagingWrite))
        {
            return;
        }

        var result = await TemplateAdminService.SaveBodyAsync(TenantId, TemplateId, PageKrafterJson.Serialize(document));
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        savedAt = $"{T[InfernoCmsLocalizableStrings.Pages.Saved].Value} {DateTime.Now:HH:mm:ss}";
        Toasts.Success(T[InfernoCmsLocalizableStrings.Pages.Saved]);
    }
}
