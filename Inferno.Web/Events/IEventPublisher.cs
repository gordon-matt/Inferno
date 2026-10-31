namespace Inferno.Web.Events;

public interface IEventPublisher
{
    void Publish<T>(T eventMessage);
}
