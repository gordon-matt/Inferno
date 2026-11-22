using Inferno.Security.Membership;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Events;

/// <summary>Raised after a subscriber is opted out. The account is not deleted.</summary>
public class NewsletterUnsubscribedEvent
{
    /// <summary>Stores the account that was opted out.</summary>
    /// <param name="user">The account.</param>
    public NewsletterUnsubscribedEvent(InfernoUser user)
    {
        User = user;
    }

    /// <summary>The account that was opted out.</summary>
    public InfernoUser User { get; }
}
