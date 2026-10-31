using System.Net.Mail;
using System.Text;
using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.Messaging.Entities;

namespace Inferno.Web.Messaging.Services;

public interface IMessageService : IGenericDataService<QueuedEmail>
{
    Guid SendEmailMessage(int tenantId, string subject, string body, string toEmailAddress, string toName = null);

    Guid SendEmailMessage(int tenantId, MailMessage mailMessage);

    Guid SendEmailMessage(
        int tenantId,
        string templateName,
        string cultureCode,
        IEnumerable<Token> tokens,
        string toEmailAddress,
        string toName = null);
}

public class MessageService : GenericDataService<QueuedEmail>, IMessageService, IQueuedMessageProvider
{
    private readonly IMessageTemplateService messageTemplateService;
    private readonly IMessageTemplateVersionService messageTemplateVersionService;
    private readonly ITokenizer tokenizer;

    public MessageService(
        ICacheManager cacheManager,
        IRepository<QueuedEmail> queuedEmailRepository,
        IMessageTemplateService messageTemplateService,
        IMessageTemplateVersionService messageTemplateVersionService,
        ITokenizer tokenizer)
        : base(cacheManager, queuedEmailRepository)
    {
        this.messageTemplateService = messageTemplateService;
        this.messageTemplateVersionService = messageTemplateVersionService;
        this.tokenizer = tokenizer;
    }

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
        string body = tokenizer.Replace(version.Data, tokens, htmlEncode: true);
        return SendEmailMessage(tenantId, subject, body, toEmailAddress, toName);
    }

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

    public void OnSendSuccess(IMailMessage mailMessage)
    {
        var entity = (QueuedEmail)mailMessage;
        entity.SentOnUtc = DateTime.UtcNow;
        Update(entity);
    }

    public void OnSendError(IMailMessage mailMessage)
    {
        var entity = (QueuedEmail)mailMessage;
        entity.SentTries++;
        Update(entity);
    }
}
