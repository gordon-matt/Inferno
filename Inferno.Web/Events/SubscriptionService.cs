using Dependo;

namespace Inferno.Web.Events;

/// <inheritdoc cref="ISubscriptionService" />
public class SubscriptionService : ISubscriptionService
{
    /// <inheritdoc />
    public IList<IConsumer<T>> GetSubscriptions<T>() => DependoResolver.Instance.ResolveAll<IConsumer<T>>().ToList();
}