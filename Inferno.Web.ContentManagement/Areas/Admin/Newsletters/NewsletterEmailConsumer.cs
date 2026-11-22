using Dependo;
using Inferno.Web.ContentManagement.Areas.Admin.Messaging;
using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Events;
using Inferno.Web.Events;
using Inferno.Web.Messaging;
using Inferno.Web.Messaging.Entities;
using Inferno.Web.Messaging.Services;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters;

/// <summary>
/// Sends the subscribe or unsubscribe email. A missing template is created for the current tenant first.
/// Nothing is sent when the user, the email, or the current tenant is missing.
/// </summary>
public class NewsletterEmailConsumer :
    IConsumer<NewsletterSubscribedEvent>,
    IConsumer<NewsletterUnsubscribedEvent>
{
    /// <summary>The name of the message template sent after a subscription.</summary>
    public const string SubscribedTemplate = "Newsletter: Subscribed";

    /// <summary>The name of the message template sent after an opt-out.</summary>
    public const string UnsubscribedTemplate = "Newsletter: Unsubscribed";

    private readonly IMessageService messageService;
    private readonly IWorkContext workContext;

    /// <summary>Stores the message service and the work context.</summary>
    /// <param name="messageService">Sends the email.</param>
    /// <param name="workContext">Supplies the current tenant and culture. A missing tenant skips the send.</param>
    public NewsletterEmailConsumer(IMessageService messageService, IWorkContext workContext)
    {
        this.messageService = messageService;
        this.workContext = workContext;
    }

    /// <summary>Sends <see cref="SubscribedTemplate"/>, including <see cref="NewsletterSubscribedEvent.Password"/> as a token.</summary>
    /// <param name="eventMessage">The subscription. A null user or blank email is ignored.</param>
    public void HandleEvent(NewsletterSubscribedEvent eventMessage) =>
        Send(SubscribedTemplate, eventMessage.User, eventMessage.Password);

    /// <summary>Sends <see cref="UnsubscribedTemplate"/> with an empty password token.</summary>
    /// <param name="eventMessage">The opt-out. A null user or blank email is ignored.</param>
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