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
            new("[UserName]", user.UserName ?? user.Email),
            new("[Email]", user.Email),
            new("[Password]", password ?? string.Empty)
        };

        int tenantId = workContext.CurrentTenant.Id;
        if (templateName == SubscribedTemplate)
        {
            EnsureTemplate(tenantId, SubscribedTemplate, "Thank you for subscribing", "<p>Thank you, [UserName]. You are signed up at [Email].</p>");
        }
        else if (templateName == UnsubscribedTemplate)
        {
            EnsureTemplate(tenantId, UnsubscribedTemplate, "You have unsubscribed", "<p>[UserName], you have been removed from the mailing list.</p>");
        }

        messageService.SendEmailMessage(
            workContext.CurrentTenant.Id,
            templateName,
            workContext.CurrentCultureCode,
            tokens,
            user.Email,
            user.UserName);
    }

    private void EnsureTemplate(int tenantId, string name, string subject, string body)
    {
        var templateService = Dependo.DependoResolver.Instance.Resolve<IMessageTemplateService>();
        if (templateService.Find(tenantId, name) != null)
        {
            return;
        }

        var template = templateService.Insert(new MessageTemplate
        {
            TenantId = tenantId,
            Name = name,
            Editor = "Html",
            Enabled = true
        });

        var versionService = Dependo.DependoResolver.Instance.Resolve<IMessageTemplateVersionService>();
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
