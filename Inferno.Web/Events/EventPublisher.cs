using Dependo;
using Inferno.Plugins;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Events;

public class EventPublisher : IEventPublisher
{
    private readonly ISubscriptionService subscriptionService;

    public EventPublisher(ISubscriptionService subscriptionService)
    {
        this.subscriptionService = subscriptionService;
    }

    public virtual void Publish<T>(T eventMessage)
    {
        var subscribers = subscriptionService.GetSubscriptions<T>()
            .Where(subscriber => PluginManager.FindPlugin(subscriber.GetType())?.Installed ?? true)
            .ToList();

        foreach (var subscriber in subscribers)
        {
            try
            {
                subscriber.HandleEvent(eventMessage);
            }
            catch (Exception ex)
            {
                try
                {
                    var loggerFactory = DependoResolver.Instance.Resolve<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger(GetType());
                    logger.LogError(ex, "Event handler {Handler} failed for {EventType}", subscriber.GetType().Name, typeof(T).Name);
                }
                catch (Exception)
                {
                    // A logging failure must not hide the original handler failure or stop other subscribers.
                }
            }
        }
    }
}
