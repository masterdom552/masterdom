namespace Masterdom.Platform.Events;

/// <summary>
/// Defines idempotency hooks for handler execution.
/// </summary>
public interface IEventIdempotencyTracker
{
    bool HasProcessed(EventId eventId, string handlerId);

    void MarkProcessed(EventId eventId, string handlerId);
}
