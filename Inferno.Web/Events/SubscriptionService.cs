using Dependo;

namespace Inferno.Web.Events;

public class SubscriptionService : ISubscriptionService
{
    public IList<IConsumer<T>> GetSubscriptions<T>() => DependoResolver.Instance.ResolveAll<IConsumer<T>>().ToList();
}