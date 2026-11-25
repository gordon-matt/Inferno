using System.Globalization;
using System.Net.Mail;
using System.Text;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.Messaging;
using Inferno.Web.Messaging.Entities;
using Inferno.Web.Messaging.Services;
using Kourier.Core;

namespace InfernoCMS.Infrastructure.Kourier;

/// <summary>
/// Queues mail through Kourier. Keeps the old <see cref="QueuedEmail"/> table readable for any remaining
/// admin tools, but new sends go to Kourier's queue.
/// </summary>
public sealed class KourierMessageServiceShim(
    ICacheManager cacheManager,
    IRepository<QueuedEmail> queuedEmailRepository,
    IKourierEmailService email) : GenericDataService<QueuedEmail>(cacheManager, queuedEmailRepository), IMessageService, IQueuedMessageProvider
{
    /// <inheritdoc />
    public Guid SendEmailMessage(int tenantId, string subject, string body, string toEmailAddress, string toName = null)
    {
        var mailMessage = new MailMessage
        {
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
            Subject = subject ?? string.Empty,
            Body = body ?? string.Empty,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(new MailAddress(toEmailAddress, toName ?? string.Empty));
        return SendEmailMessage(tenantId, mailMessage);
    }

    /// <inheritdoc />
    public Guid SendEmailMessage(int tenantId, MailMessage mailMessage)
    {
        ArgumentNullException.ThrowIfNull(mailMessage);
        if (mailMessage.To.Count == 0)
        {
            return Guid.Empty;
        }

        var to = mailMessage.To[0];
        var result = email.SendAsync(
            new KourierEmail
            {
                To = KourierRecipient.Email(to.Address, to.DisplayName),
                Subject = mailMessage.Subject ?? string.Empty,
                HtmlBody = mailMessage.IsBodyHtml ? mailMessage.Body : null,
                TextBody = mailMessage.IsBodyHtml ? null : mailMessage.Body,
            },
            new KourierSendOptions
            {
                TenantId = tenantId.ToString(CultureInfo.InvariantCulture),
                From = mailMessage.From is null ? null : new KourierAddress(mailMessage.From.Address, mailMessage.From.DisplayName),
                Cc = mailMessage.CC.Select(address => new KourierAddress(address.Address, address.DisplayName)).ToList(),
                Bcc = mailMessage.Bcc.Select(address => new KourierAddress(address.Address, address.DisplayName)).ToList(),
            }).GetAwaiter().GetResult();

        return result.IsSuccess ? result.Value : Guid.Empty;
    }

    /// <inheritdoc />
    public Guid SendEmailMessage(
        int tenantId,
        string templateName,
        string cultureCode,
        IEnumerable<Token> tokens,
        string toEmailAddress,
        string toName = null)
    {
        var model = tokens?
            .Where(token => !string.IsNullOrWhiteSpace(token.Key))
            .ToDictionary(token => token.Key, token => (object?)token.Value, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        var result = email.SendTemplateAsync(
            templateName,
            KourierRecipient.Email(toEmailAddress, toName),
            model,
            new KourierSendOptions
            {
                TenantId = tenantId.ToString(CultureInfo.InvariantCulture),
                Culture = string.IsNullOrWhiteSpace(cultureCode) ? null : cultureCode,
            }).GetAwaiter().GetResult();

        return result.IsSuccess ? result.Value : Guid.Empty;
    }

    /// <inheritdoc />
    public IEnumerable<IMailMessage> GetQueuedEmails(int tenantId, int maxSendTries, int maxMessageItems) => [];

    /// <inheritdoc />
    public void OnSendSuccess(IMailMessage mailMessage)
    {
    }

    /// <inheritdoc />
    public void OnSendError(IMailMessage mailMessage)
    {
    }
}