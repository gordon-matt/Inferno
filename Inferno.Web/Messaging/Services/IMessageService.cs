using System.Net.Mail;
using System.Text;
using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.Messaging.Entities;

namespace Inferno.Web.Messaging.Services;

/// <summary>Queues mail. Nothing is sent until <see cref="Inferno.Web.Messaging.ProcessQueuedMailTask"/> runs.</summary>
public interface IMessageService : IGenericDataService<QueuedEmail>
{
    /// <summary>Queues an HTML message. The body is not encoded again.</summary>
    /// <param name="tenantId">The tenant the row is stored for.</param>
    /// <param name="subject">The subject. <see langword="null"/> is stored as empty.</param>
    /// <param name="body">The HTML body. <see langword="null"/> is stored as empty.</param>
    /// <param name="toEmailAddress">The recipient address.</param>
    /// <param name="toName">The recipient display name. <see langword="null"/> is stored as empty.</param>
    /// <returns>The queued row id.</returns>
    Guid SendEmailMessage(int tenantId, string subject, string body, string toEmailAddress, string toName = null);

    /// <summary>Queues <paramref name="mailMessage"/> at priority 5. The first recipient is copied onto the row.</summary>
    /// <param name="tenantId">The tenant the row is stored for.</param>
    /// <param name="mailMessage">The message to store. Its first recipient is required.</param>
    /// <returns>The queued row id.</returns>
    Guid SendEmailMessage(int tenantId, MailMessage mailMessage);

    /// <summary>
    /// Queues a message from an enabled template. The requested culture is used, then a version with no culture.
    /// The subject is filled without HTML encoding. The body is passed to the first <see cref="IMessageTemplateRenderer"/>
    /// that returns HTML, then filled with HTML encoding.
    /// </summary>
    /// <param name="tenantId">The tenant whose template is loaded.</param>
    /// <param name="templateName">The template name.</param>
    /// <param name="cultureCode">The preferred culture. Empty skips the culture match and uses the version with no culture.</param>
    /// <param name="tokens">Values substituted into the subject and body.</param>
    /// <param name="toEmailAddress">The recipient address.</param>
    /// <param name="toName">The recipient display name. <see langword="null"/> is stored as empty.</param>
    /// <returns>The queued row id, or <see cref="Guid.Empty"/> when the template is missing, disabled, or has no matching version.</returns>
    Guid SendEmailMessage(
        int tenantId,
        string templateName,
        string cultureCode,
        IEnumerable<Token> tokens,
        string toEmailAddress,
        string toName = null);
}

/// <inheritdoc cref="IMessageService" />
public class MessageService : GenericDataService<QueuedEmail>, IMessageService, IQueuedMessageProvider
{
    private readonly IMessageTemplateService messageTemplateService;
    private readonly IMessageTemplateVersionService messageTemplateVersionService;
    private readonly ITokenizer tokenizer;
    private readonly IEnumerable<IMessageTemplateRenderer> templateRenderers;

    /// <summary>Creates the service.</summary>
    /// <param name="cacheManager">Passed to the data service.</param>
    /// <param name="queuedEmailRepository">The queue table.</param>
    /// <param name="messageTemplateService">Finds the template by tenant and name.</param>
    /// <param name="messageTemplateVersionService">Finds the culture version.</param>
    /// <param name="tokenizer">Fills Liquid tokens.</param>
    /// <param name="templateRenderers">Asked, in order, to turn the stored body into HTML.</param>
    public MessageService(
        ICacheManager cacheManager,
        IRepository<QueuedEmail> queuedEmailRepository,
        IMessageTemplateService messageTemplateService,
        IMessageTemplateVersionService messageTemplateVersionService,
        ITokenizer tokenizer,
        IEnumerable<IMessageTemplateRenderer> templateRenderers)
        : base(cacheManager, queuedEmailRepository)
    {
        this.messageTemplateService = messageTemplateService;
        this.messageTemplateVersionService = messageTemplateVersionService;
        this.tokenizer = tokenizer;
        this.templateRenderers = templateRenderers;
    }

    /// <inheritdoc />
    public Guid SendEmailMessage(int tenantId, string subject, string body, string toEmailAddress, string toName = null)
    {
        var mailMessage = new MailMessage
        {
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
            Subject = subject ?? string.Empty,
            Body = body ?? string.Empty,
            IsBodyHtml = true
        };
        mailMessage.To.Add(new MailAddress(toEmailAddress, toName ?? string.Empty));
        return SendEmailMessage(tenantId, mailMessage);
    }

    /// <inheritdoc />
    public Guid SendEmailMessage(int tenantId, MailMessage mailMessage)
    {
        var queuedEmail = new QueuedEmail
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Priority = 5,
            FromAddress = mailMessage.From?.Address,
            FromName = mailMessage.From?.DisplayName,
            ToAddress = mailMessage.To[0].Address,
            ToName = mailMessage.To[0].DisplayName,
            Subject = mailMessage.Subject,
            MailMessage = MailMessageWrapper.FromMailMessage(mailMessage).ToString(),
            CreatedOnUtc = DateTime.UtcNow
        };

        Insert(queuedEmail);
        return queuedEmail.Id;
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
        var template = messageTemplateService.Find(tenantId, templateName);
        if (template == null || !template.Enabled)
        {
            return Guid.Empty;
        }

        var version = messageTemplateVersionService.FindOne(template.Id, cultureCode);
        if (version == null)
        {
            return Guid.Empty;
        }

        string subject = tokenizer.Replace(version.Subject, tokens, htmlEncode: false);
        string bodySource = version.Data ?? string.Empty;
        foreach (var renderer in templateRenderers)
        {
            string rendered = renderer.ToHtml(bodySource, version.Subject);
            if (rendered != null)
            {
                bodySource = rendered;
                break;
            }
        }

        string body = tokenizer.Replace(bodySource, tokens, htmlEncode: true);
        return SendEmailMessage(tenantId, subject, body, toEmailAddress, toName);
    }

    /// <inheritdoc />
    public IEnumerable<IMailMessage> GetQueuedEmails(int tenantId, int maxSendTries, int maxMessageItems)
    {
        using var connection = OpenConnection();
        return connection
            .Query(x =>
                x.TenantId == tenantId
                && x.SentTries < maxSendTries
                && x.SentOnUtc == null)
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.CreatedOnUtc)
            .Take(maxMessageItems)
            .ToList();
    }

    /// <inheritdoc />
    public void OnSendSuccess(IMailMessage mailMessage)
    {
        var entity = (QueuedEmail)mailMessage;
        entity.SentOnUtc = DateTime.UtcNow;
        Update(entity);
    }

    /// <inheritdoc />
    public void OnSendError(IMailMessage mailMessage)
    {
        var entity = (QueuedEmail)mailMessage;
        entity.SentTries++;
        Update(entity);
    }
}
