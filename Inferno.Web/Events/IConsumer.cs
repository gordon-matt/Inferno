namespace Inferno.Web.Events;

/// <summary>
/// Handles one published event. A class may implement this for more than one event type.
/// </summary>
public interface IConsumer<T>
{
    void HandleEvent(T eventMessage);
}