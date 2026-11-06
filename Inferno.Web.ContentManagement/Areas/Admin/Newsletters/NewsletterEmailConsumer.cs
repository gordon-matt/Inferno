using Dependo;
using Inferno.Web.ContentManagement.Areas.Admin.Messaging;
using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Events;
using Inferno.Web.Events;
using Inferno.Web.Messaging;
using Inferno.Web.Messaging.Entities;
using Inferno.Web.Messaging.Services;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters;

public class NewsletterEmailConsumer :
    IConsumer<NewsletterSubscribedEvent>,
    IConsumer<NewsletterUnsubscribedEvent>
{
    public const string SubscribedTemplate = "Newsletter: Subscribed";
    public const string UnsubscribedTemplate = "Newsletter: Unsubscribed";

    private readonly IMessageService messageService;
    private readonly IWorkContext workContext;

    public NewsletterEmailConsumer(IMessageService messageService, IWorkContext workContext)
    {
        this.messageService = messageService;
        this.workContext = workContext;
    }

    public void HandleEvent(NewsletterSubscribedEvent eventMessage) =>
        Send(SubscribedTemplate, eventMessage.User, eventMessage.Password);

    public void HandleEvent(NewsletterUnsubscribedEvent eventMessage) =>
        Send(UnsubscribedTemplate, eventMessage.User, string.Empty);

    private void Send(string templateName, global::Inferno.Security.Membership.InfernoUser user, string password)
    {
        if (user == null || string.IsNullOrWhiteSpace(user.Email) || workContext.CurrentTenant == null)
        {
            return;
        }

        var tokens = new List<Token>
        {
            new("UserName", user.UserName ?? user.Email),
            new("Email", user.Email),
            new("Password", password ?? string.Empty)
        };

        int tenantId = workContext.CurrentTenant.Id;
        if (templateName == SubscribedTemplate)
        {
            EnsureTemplate(tenantId, SubscribedTemplate, "Thank you for subscribing", MessageTemplateDocuments.Email("<p>Thank you, {{ UserName }}. You are signed up at {{ Email }}.</p>"));
        }
        else if (templateName == UnsubscribedTemplate)
        {
            EnsureTemplate(tenantId, UnsubscribedTemplate, "You have unsubscribed", MessageTemplateDocuments.Email("<p>{{ UserName }}, you have been removed from the mailing list.</p>"));
        }

        messageService.SendEmailMessage(
            workContext.CurrentTenant.Id,
            templateName,
            workContext.CurrentCultureCode,
            tokens,
            user.Email,
            user.UserName);
    }

    private static readonly string[] NewsletterTokenList = ["{{ UserName }}", "{{ Email }}", "{{ Password }}"];

    private void EnsureTemplate(int tenantId, string name, string subject, string body)
    {
        var templateService = DependoResolver.Instance.Resolve<IMessageTemplateService>();
        string tokenJson = MessageTemplateTokens.Write(NewsletterTokenList);
        var existing = templateService.Find(tenantId, name);
        if (existing != null)
        {
            if (string.IsNullOrWhiteSpace(existing.Tokens))
            {
                existing.Tokens = tokenJson;
                templateService.Update(existing);
            }

            return;
        }

        var template = templateService.Insert(new MessageTemplate
        {
            TenantId = tenantId,
            Name = name,
            Enabled = true,
            Tokens = tokenJson
        });

        var versionService = DependoResolver.Instance.Resolve<IMessageTemplateVersionService>();
        versionService.Insert(new MessageTemplateVersion
        {
            MessageTemplateId = template.Id,
            CultureCode = null,
            Subject = subject,
            Data = body,
            DateCreatedUtc = DateTime.UtcNow,
            DateModifiedUtc = DateTime.UtcNow
        });
    }
}