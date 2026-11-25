using System.Net.Mail;
using System.Text;
using Heroicsoft.NetForge.Results;
using Inferno.Web.Messaging.Services;
using Kourier.Core;

namespace InfernoCMS.Infrastructure.Kourier;

/// <summary>Sends form-block and other immediate mail through Kourier instead of System.Net.Mail SMTP.</summary>
public sealed class KourierEmailSenderShim(IKourierEmailService email) : IEmailSender
{
    /// <inheritdoc />
    public void Send(MailMessage mailMessage)
    {
        ArgumentNullException.ThrowIfNull(mailMessage);
        if (mailMessage.To.Count == 0)
        {
            throw new InvalidOperationException("The message has no recipients.");
        }

        var to = mailMessage.To[0];
        var options = new KourierSendOptions
        {
            From = mailMessage.From is null
                ? null
                : new KourierAddress(mailMessage.From.Address, mailMessage.From.DisplayName),
            ReplyTo = mailMessage.ReplyToList.Count > 0
                ? new KourierAddress(mailMessage.ReplyToList[0].Address, mailMessage.ReplyToList[0].DisplayName)
                : null,
            Cc = mailMessage.CC.Select(address => new KourierAddress(address.Address, address.DisplayName)).ToList(),
            Bcc = mailMessage.Bcc.Select(address => new KourierAddress(address.Address, address.DisplayName)).ToList(),
            Category = Constants.Categories.Transactional,
        };

        var result = email.SendNowAsync(
            new KourierEmail
            {
                To = KourierRecipient.Email(to.Address, to.DisplayName),
                Subject = mailMessage.Subject ?? string.Empty,
                HtmlBody = mailMessage.IsBodyHtml ? mailMessage.Body : null,
                TextBody = mailMessage.IsBodyHtml ? null : mailMessage.Body,
            },
            options).GetAwaiter().GetResult();

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.GetDisplayMessage() ?? "The email could not be sent.");
        }
    }

    /// <inheritdoc />
    public void Send(string subject, string body, string toEmailAddress)
    {
        using var mailMessage = new MailMessage
        {
            Subject = subject,
            SubjectEncoding = Encoding.UTF8,
            Body = body,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(toEmailAddress);
        Send(mailMessage);
    }
}