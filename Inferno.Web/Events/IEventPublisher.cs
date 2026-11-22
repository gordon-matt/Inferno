namespace Inferno.Web.Events;

/// <summary>Sends one event to every active subscriber.</summary>
public interface IEventPublisher
{
    /// <summary>
    /// Calls <see cref="IConsumer{T}.HandleEvent"/> on each subscriber of <typeparamref name="T"/>.
    /// A subscriber that belongs to an inactive plugin is skipped. A handler that throws is logged and the remaining handlers still run.
    /// </summary>
    /// <typeparam name="T">The event type.</typeparam>
    /// <param name="eventMessage">The event passed to each handler.</param>
    void Publish<T>(T eventMessage);
}