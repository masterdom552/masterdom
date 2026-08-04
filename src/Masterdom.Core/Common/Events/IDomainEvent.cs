namespace Masterdom.Core.Common.Events;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
