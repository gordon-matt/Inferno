namespace Inferno.Web.Events;

/// <summary>
/// Handles one published event. A class may implement this for more than one event type.
/// </summary>
/// <typeparam name="T">The event type.</typeparam>
public interface IConsumer<T>
{
    /// <summary>Handles <paramref name="eventMessage"/>.</summary>
    /// <param name="eventMessage">The published event.</param>
    void HandleEvent(T eventMessage);
}