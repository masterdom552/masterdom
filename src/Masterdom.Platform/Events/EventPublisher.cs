using System;

namespace Masterdom.Platform.Events;

/// <summary>
/// Captures and dispatches platform events.
/// </summary>
public sealed class EventPublisher : IEventPublisher
{
    private readonly IEventStore _eventStore;
    private readonly IEventDispatcher _dispatcher;

    public EventPublisher(IEventStore eventStore, IEventDispatcher dispatcher)
    {
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public EventPublishResult Publish(EventEnvelope envelope, EventDispatchPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        _eventStore.Append(envelope);
        var dispatchResult = _dispatcher.Dispatch(envelope, policy);

        return new EventPublishResult
        {
            Envelope = envelope,
            Dispatch = dispatchResult
        };
    }

    public EventPublishResult Publish(IPlatformEvent platformEvent, EventContext context, EventDispatchPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(platformEvent);
        ArgumentNullException.ThrowIfNull(context);

        var envelope = new EventEnvelope(platformEvent, context);
        return Publish(envelope, policy);
    }
}
