namespace Masterdom.Platform.Events;

/// <summary>
/// Publishes events through the dispatch pipeline.
/// </summary>
public interface IEventPublisher
{
    EventPublishResult Publish(EventEnvelope envelope, EventDispatchPolicy? policy = null);

    EventPublishResult Publish(IPlatformEvent platformEvent, EventContext context, EventDispatchPolicy? policy = null);
}
