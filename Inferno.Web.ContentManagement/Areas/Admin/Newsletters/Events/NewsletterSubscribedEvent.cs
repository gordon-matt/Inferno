using Inferno.Security.Membership;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Events;

/// <summary>Raised after an address is subscribed. A new account includes its generated password.</summary>
public class NewsletterSubscribedEvent
{
    /// <summary>Stores the subscriber and the password, using an empty password when none was supplied.</summary>
    /// <param name="user">The subscribed account.</param>
    /// <param name="password">The generated password for a new account. <see langword="null"/> is stored as empty. An existing account passes none.</param>
    public NewsletterSubscribedEvent(InfernoUser user, string password = null)
    {
        User = user;
        Password = password ?? string.Empty;
    }

    /// <summary>The subscribed account.</summary>
    public InfernoUser User { get; }

    /// <summary>The generated password, or empty when the account already existed.</summary>
    public string Password { get; }
}
