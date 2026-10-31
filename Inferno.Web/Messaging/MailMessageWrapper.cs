using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace Inferno.Web.Messaging;

/// <summary>
/// JSON round-trip of the fields the queue needs to resend a message.
/// Attachments are omitted; form-block mail is sent immediately and does not go through the queue.
/// </summary>
public class MailMessageWrapper
{
    public string FromAddress { get; set; }

    public string FromName { get; set; }

    public List<string> To { get; set; } = [];

    public string Subject { get; set; }

    public string Body { get; set; }

    public bool IsBodyHtml { get; set; }

    public static MailMessageWrapper FromMailMessage(MailMessage mailMessage)
    {
        ArgumentNullException.ThrowIfNull(mailMessage);

        return new MailMessageWrapper
        {
            FromAddress = mailMessage.From?.Address,
            FromName = mailMessage.From?.DisplayName,
            To = mailMessage.To.Select(x => x.Address).ToList(),
            Subject = mailMessage.Subject,
            Body = mailMessage.Body,
            IsBodyHtml = mailMessage.IsBodyHtml
        };
    }

    public static MailMessageWrapper FromJson(string json) =>
        string.IsNullOrWhiteSpace(json)
            ? new MailMessageWrapper()
            : JsonSerializer.Deserialize<MailMessageWrapper>(json) ?? new MailMessageWrapper();

    public MailMessage ToMailMessage()
    {
        var message = new MailMessage
        {
            Subject = Subject ?? string.Empty,
            Body = Body ?? string.Empty,
            IsBodyHtml = IsBodyHtml,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };

        if (!string.IsNullOrWhiteSpace(FromAddress))
        {
            message.From = new MailAddress(FromAddress, FromName ?? string.Empty);
        }

        foreach (string address in To ?? [])
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                message.To.Add(address);
            }
        }

        return message;
    }

    public override string ToString() => JsonSerializer.Serialize(this);
}
