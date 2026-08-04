namespace Masterdom.Core.Common.Events;

public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
