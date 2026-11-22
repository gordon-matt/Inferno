using Dependo;
using Microsoft.Extensions.Logging;
using Pluginz.Registry;

namespace Inferno.Web.Events;

/// <inheritdoc cref="IEventPublisher" />
public class EventPublisher : IEventPublisher
{
    private readonly ISubscriptionService subscriptionService;
    private readonly IPluginRegistry plugins;

    /// <summary>Creates the publisher.</summary>
    /// <param name="subscriptionService">Resolves the handlers for an event type.</param>
    /// <param name="plugins">Used to skip a handler whose plugin is inactive.</param>
    public EventPublisher(ISubscriptionService subscriptionService, IPluginRegistry plugins)
    {
        this.subscriptionService = subscriptionService;
        this.plugins = plugins;
    }

    /// <inheritdoc />
    public virtual void Publish<T>(T eventMessage)
    {
        // A subscriber that ships inside a plugin runs only while that plugin is active. Host subscribers always run.
        var subscribers = subscriptionService.GetSubscriptions<T>()
            .Where(subscriber => plugins.FindByType(subscriber.GetType()) is not { IsActive: false })
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