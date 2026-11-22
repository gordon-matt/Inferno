using System.Net.Mail;

namespace Inferno.Web.Messaging;

/// <summary>A queued message that can be turned back into mail.</summary>
public interface IMailMessage
{
    /// <summary>Builds the mail message this instance represents.</summary>
    /// <returns>The mail message.</returns>
    MailMessage GetMailMessage();
}
