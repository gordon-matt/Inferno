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
    /// <summary>The From address. Empty when the message had none.</summary>
    public string FromAddress { get; set; }

    /// <summary>The From display name.</summary>
    public string FromName { get; set; }

    /// <summary>Recipient addresses. Attachments are not stored.</summary>
    public List<string> To { get; set; } = [];

    /// <summary>The subject.</summary>
    public string Subject { get; set; }

    /// <summary>The body.</summary>
    public string Body { get; set; }

    /// <summary>Whether <see cref="Body"/> is HTML.</summary>
    public bool IsBodyHtml { get; set; }

    /// <summary>Copies the fields the queue resends. Attachments are omitted.</summary>
    /// <param name="mailMessage">The message to copy.</param>
    /// <returns>The wrapper.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mailMessage"/> is <see langword="null"/>.</exception>
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

    /// <summary>Deserializes <paramref name="json"/>. Blank JSON, or JSON that deserializes to null, returns an empty wrapper.</summary>
    /// <param name="json">The stored JSON.</param>
    /// <returns>The wrapper.</returns>
    public static MailMessageWrapper FromJson(string json) =>
        string.IsNullOrWhiteSpace(json)
            ? new MailMessageWrapper()
            : JsonSerializer.Deserialize<MailMessageWrapper>(json) ?? new MailMessageWrapper();

    /// <summary>Builds a UTF-8 mail message. A blank From address is left unset, and blank recipients are skipped.</summary>
    /// <returns>The mail message.</returns>
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

    /// <summary>Serializes this wrapper to JSON.</summary>
    /// <returns>The JSON.</returns>
    public override string ToString() => JsonSerializer.Serialize(this);
}
