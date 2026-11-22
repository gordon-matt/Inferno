using Ardalis.Result;
using Inferno.Web.Messaging.Entities;
using Inferno.Web.Messaging.Services;
using Inferno.Web.Models;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.ContentManagement.Areas.Admin.Messaging.Services;

/// <summary>Which queued messages a search includes.</summary>
public enum QueuedEmailStatus
{
    /// <summary>Waiting and sent messages.</summary>
    All,

    /// <summary>Messages that have not been sent.</summary>
    Waiting,

    /// <summary>Messages that have a sent time.</summary>
    Sent,
}

/// <summary>One queued message for the list.</summary>
/// <param name="Id">The queued message identifier.</param>
/// <param name="ToAddress">The recipient address.</param>
/// <param name="ToName">The recipient name.</param>
/// <param name="Subject">The subject.</param>
/// <param name="CreatedOnUtc">When the message was queued, in UTC.</param>
/// <param name="SentTries">How many send attempts have been made.</param>
/// <param name="SentOnUtc">When the message was sent, in UTC. <see langword="null"/> while it is waiting.</param>
public sealed record QueuedEmailSummary(Guid Id, string ToAddress, string ToName, string Subject, DateTime CreatedOnUtc, int SentTries, DateTime? SentOnUtc);

/// <summary>A queued message opened for reading.</summary>
/// <param name="Summary">The list fields.</param>
/// <param name="FromAddress">The sender address.</param>
/// <param name="FromName">The sender name.</param>
/// <param name="Body">The message body, or <see langword="null"/> when it cannot be read.</param>
/// <param name="IsBodyHtml"><see langword="true"/> when <paramref name="Body"/> is HTML.</param>
public sealed record QueuedEmailMessage(QueuedEmailSummary Summary, string FromAddress, string FromName, string Body, bool IsBodyHtml);

/// <summary>Reads and deletes a tenant's queued email.</summary>
public interface IQueuedEmailAdminService
{
    /// <summary>Lists the tenant's queue, newest first. <see cref="ListQuery.Search"/> matches the recipient or subject.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="query">The page size, skip, and search text.</param>
    /// <param name="status">Which messages to include. Any value other than waiting or sent includes both.</param>
    /// <returns>The page of messages and the total that matched before paging.</returns>
    Task<Result<ListPage<QueuedEmailSummary>>> SearchAsync(int tenantId, ListQuery query, QueuedEmailStatus status);

    /// <summary>Loads one queued message, including its body when the stored mail can be read.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="id">The queued message.</param>
    /// <returns>The message, or not found. A body that cannot be read is <see langword="null"/>.</returns>
    Task<Result<QueuedEmailMessage>> GetAsync(int tenantId, Guid id);

    /// <summary>Deletes a queued message.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="id">The queued message.</param>
    /// <returns>Success, or not found.</returns>
    Task<Result> DeleteAsync(int tenantId, Guid id);
}

/// <inheritdoc cref="IQueuedEmailAdminService" />
/// <param name="messageService">Reads and deletes queued messages.</param>
/// <param name="logger">Records a message whose body cannot be read.</param>
public class QueuedEmailAdminService(
    IMessageService messageService,
    ILogger<QueuedEmailAdminService> logger) : IQueuedEmailAdminService
{
    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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
