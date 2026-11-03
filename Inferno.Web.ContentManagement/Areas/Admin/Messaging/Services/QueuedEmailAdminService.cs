using Ardalis.Result;
using Inferno.Web.Messaging.Entities;
using Inferno.Web.Messaging.Services;
using Inferno.Web.Models;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.ContentManagement.Areas.Admin.Messaging.Services;

public enum QueuedEmailStatus
{
    All,
    Waiting,
    Sent,
}

public sealed record QueuedEmailSummary(Guid Id, string ToAddress, string ToName, string Subject, DateTime CreatedOnUtc, int SentTries, DateTime? SentOnUtc);

public sealed record QueuedEmailMessage(QueuedEmailSummary Summary, string FromAddress, string FromName, string Body, bool IsBodyHtml);

public interface IQueuedEmailAdminService
{
    /// <summary>The tenant's queue, newest first. <see cref="ListQuery.Search"/> matches the recipient or subject.</summary>
    Task<Result<ListPage<QueuedEmailSummary>>> SearchAsync(int tenantId, ListQuery query, QueuedEmailStatus status);

    Task<Result<QueuedEmailMessage>> GetAsync(int tenantId, Guid id);

    Task<Result> DeleteAsync(int tenantId, Guid id);
}

public class QueuedEmailAdminService(
    IMessageService messageService,
    ILogger<QueuedEmailAdminService> logger) : IQueuedEmailAdminService
{
    public Task<Result<ListPage<QueuedEmailSummary>>> SearchAsync(int tenantId, ListQuery query, QueuedEmailStatus status)
    {
        using var connection = messageService.OpenConnection();
        var emails = connection.Query(email => email.TenantId == tenantId);

        emails = status switch
        {
            QueuedEmailStatus.Waiting => emails.Where(email => email.SentOnUtc == null),
            QueuedEmailStatus.Sent => emails.Where(email => email.SentOnUtc != null),
            _ => emails,
        };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim();
            emails = emails.Where(email => email.ToAddress.Contains(term) || email.ToName.Contains(term) || email.Subject.Contains(term));
        }

        int total = emails.Count();
        var items = emails
            .OrderByDescending(email => email.CreatedOnUtc)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(email => new QueuedEmailSummary(email.Id, email.ToAddress, email.ToName, email.Subject, email.CreatedOnUtc, email.SentTries, email.SentOnUtc))
            .ToList();

        return Task.FromResult(Result.Success(new ListPage<QueuedEmailSummary>(items, total)));
    }

    public async Task<Result<QueuedEmailMessage>> GetAsync(int tenantId, Guid id)
    {
        var email = await messageService.FindOneAsync(id);
        if (email is null || email.TenantId != tenantId)
        {
            return Result<QueuedEmailMessage>.NotFound();
        }

        string body = null;
        bool isHtml = false;
        try
        {
            using var message = email.GetMailMessage();
            body = message.Body;
            isHtml = message.IsBodyHtml;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Queued email {EmailId} has a message that cannot be read", id);
        }

        var summary = new QueuedEmailSummary(email.Id, email.ToAddress, email.ToName, email.Subject, email.CreatedOnUtc, email.SentTries, email.SentOnUtc);
        return Result<QueuedEmailMessage>.Success(new QueuedEmailMessage(summary, email.FromAddress, email.FromName, body, isHtml));
    }

    public async Task<Result> DeleteAsync(int tenantId, Guid id)
    {
        var email = await messageService.FindOneAsync(id);
        if (email is null || email.TenantId != tenantId)
        {
            return Result.NotFound();
        }

        await messageService.DeleteAsync(email);
        return Result.Success();
    }
}
