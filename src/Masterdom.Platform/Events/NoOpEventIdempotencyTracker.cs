namespace Masterdom.Platform.Events;

/// <summary>
/// No-op idempotency tracker used until durable idempotency storage is introduced.
/// </summary>
public sealed class NoOpEventIdempotencyTracker : IEventIdempotencyTracker
{
    public bool HasProcessed(EventId eventId, string handlerId)
    {
        return false;
    }

    public void MarkProcessed(EventId eventId, string handlerId)
    {
    }
}
