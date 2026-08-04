using DomainEventContract = Masterdom.Core.Common.Events.IDomainEvent;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents a domain event adapted into the platform event model.
/// </summary>
public interface IDomainRuntimeEvent : IEvent
{
    DomainEventContract DomainEvent { get; }
}
