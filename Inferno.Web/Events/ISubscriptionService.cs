namespace Inferno.Web.Events;

/// <summary>Finds the handlers registered for an event type.</summary>
public interface ISubscriptionService
{
    /// <summary>The handlers registered for <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The event type.</typeparam>
    /// <returns>The resolved handlers. The list is empty when none are registered.</returns>
    IList<IConsumer<T>> GetSubscriptions<T>();
}