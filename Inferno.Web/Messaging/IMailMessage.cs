using System.Net.Mail;

namespace Inferno.Web.Messaging;

public interface IMailMessage
{
    MailMessage GetMailMessage();
}
