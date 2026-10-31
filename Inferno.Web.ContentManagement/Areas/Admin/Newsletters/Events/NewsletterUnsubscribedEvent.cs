using Inferno.Security.Membership;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Events;

public class NewsletterUnsubscribedEvent
{
    public NewsletterUnsubscribedEvent(InfernoUser user)
    {
        User = user;
    }

    public InfernoUser User { get; }
}
