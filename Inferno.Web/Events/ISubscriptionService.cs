namespace Inferno.Web.Events;

public interface ISubscriptionService
{
    IList<IConsumer<T>> GetSubscriptions<T>();
}
