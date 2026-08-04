namespace Masterdom.Platform.Events;

/// <summary>
/// Dispatches event envelopes to resolved handlers.
/// </summary>
public interface IEventDispatcher
{
    EventDispatchResult Dispatch(EventEnvelope envelope, EventDispatchPolicy? policy = null);
}
