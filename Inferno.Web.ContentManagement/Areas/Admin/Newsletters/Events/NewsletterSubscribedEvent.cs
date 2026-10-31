using Inferno.Security.Membership;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Events;

public class NewsletterSubscribedEvent
{
    public NewsletterSubscribedEvent(InfernoUser user, string password = null)
    {
        User = user;
        Password = password ?? string.Empty;
    }

    public InfernoUser User { get; }

    public string Password { get; }
}
