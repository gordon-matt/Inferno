namespace Inferno.Web.Messaging;

/// <summary>The queue <see cref="ProcessQueuedMailTask"/> drains for one tenant.</summary>
public interface IQueuedMessageProvider
{
    /// <summary>
    /// Messages for <paramref name="tenantId"/> that have not been sent and have been tried fewer than <paramref name="maxSendTries"/> times.
    /// Ordered by priority, then by when they were queued.
    /// </summary>
    /// <param name="tenantId">The tenant whose queue is read.</param>
    /// <param name="maxSendTries">Messages with this many tries already are left in the queue.</param>
    /// <param name="maxMessageItems">How many messages to return.</param>
    /// <returns>The messages to send.</returns>
    IEnumerable<IMailMessage> GetQueuedEmails(int tenantId, int maxSendTries, int maxMessageItems);

    /// <summary>Sets <see cref="Inferno.Web.Messaging.Entities.QueuedEmail.SentOnUtc"/> to the current UTC time.</summary>
    /// <param name="mailMessage">The queued row that was sent.</param>
    void OnSendSuccess(IMailMessage mailMessage);

    /// <summary>Increments <see cref="Inferno.Web.Messaging.Entities.QueuedEmail.SentTries"/>.</summary>
    /// <param name="mailMessage">The queued row that failed.</param>
    void OnSendError(IMailMessage mailMessage);
}
