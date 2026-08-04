using DomainEventContract = Masterdom.Core.Common.Events.IDomainEvent;

namespace Masterdom.Platform.Events;

/// <summary>
/// Adapts core domain events into platform runtime events.
/// </summary>
public interface IDomainEventAdapter
{
    DomainRuntimeEvent Adapt(DomainEventContract domainEvent, EventContext context);
}
