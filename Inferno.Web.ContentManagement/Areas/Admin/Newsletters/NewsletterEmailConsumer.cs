using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Events;
using Inferno.Web.Events;
using Kourier.Core;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters;

/// <summary>
/// Sends the subscribe or unsubscribe email through Kourier. Nothing is sent when the user, the email,
/// or the current tenant is missing.
/// </summary>
public class NewsletterEmailConsumer :
    IConsumer<NewsletterSubscribedEvent>,
    IConsumer<NewsletterUnsubscribedEvent>
{
    /// <summary>The name of the message template sent after a subscription.</summary>
    public const string SubscribedTemplate = "Newsletter: Subscribed";

    /// <summary>The name of the message template sent after an opt-out.</summary>
    public const string UnsubscribedTemplate = "Newsletter: Unsubscribed";

    private readonly IKourierEmailService email;
    private readonly IWorkContext workContext;

    /// <summary>Stores the email service and the work context.</summary>
    /// <param name="email">Sends through Kourier.</param>
    /// <param name="workContext">Supplies the current tenant and culture. A missing tenant skips the send.</param>
    public NewsletterEmailConsumer(IKourierEmailService email, IWorkContext workContext)
    {
        this.email = email;
        this.workContext = workContext;
    }

    /// <summary>Sends <see cref="SubscribedTemplate"/>, including <see cref="NewsletterSubscribedEvent.Password"/>.</summary>
    /// <param name="eventMessage">The subscription. A null user or blank email is ignored.</param>
    public void HandleEvent(NewsletterSubscribedEvent eventMessage) =>
        Send(SubscribedTemplate, eventMessage.User, eventMessage.Password);

    /// <summary>Sends <see cref="UnsubscribedTemplate"/> with an empty password.</summary>
    /// <param name="eventMessage">The opt-out. A null user or blank email is ignored.</param>
    public void HandleEvent(NewsletterUnsubscribedEvent eventMessage) =>
        Send(UnsubscribedTemplate, eventMessage.User, string.Empty);

    private void Send(string templateKey, global::Inferno.Security.Membership.InfernoUser user, string password)
    {
        if (user == null || string.IsNullOrWhiteSpace(user.Email) || workContext.CurrentTenant == null)
        {
            return;
        }

        var model = new NewsletterEmailModel(
            user.UserName ?? user.Email,
            user.Email,
            password ?? string.Empty);

        email.SendTemplateAsync(
            templateKey,
            KourierRecipient.Email(user.Email, user.UserName),
            model,
            new KourierSendOptions
            {
                TenantId = workContext.CurrentTenant.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Culture = workContext.CurrentCultureCode,
            }).GetAwaiter().GetResult();
    }
}

/// <summary>Values for the newsletter subscribe and unsubscribe templates.</summary>
/// <param name="UserName">Sign-in name or email.</param>
/// <param name="Email">Address.</param>
/// <param name="Password">Generated password on subscribe, otherwise empty.</param>
public sealed record NewsletterEmailModel(string UserName, string Email, string Password);
