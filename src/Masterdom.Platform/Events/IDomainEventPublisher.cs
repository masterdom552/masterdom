using Masterdom.Core.Common.Interfaces;

namespace Masterdom.Platform.Events;

/// <summary>
/// Publishes aggregate domain events through the platform dispatch pipeline.
/// </summary>
public interface IDomainEventPublisher
{
    DomainEventPublishResult Publish(IHasDomainEvents aggregate, EventContext context, EventDispatchPolicy? policy = null, bool clearAfterPublish = true);
}
