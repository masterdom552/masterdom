namespace Masterdom.Platform.Events;

/// <summary>
/// Defines a future outbox extension point.
/// </summary>
public interface IEventOutbox
{
    void Enqueue(EventEnvelope envelope);
}
