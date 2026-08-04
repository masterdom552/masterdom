using System;
using System.Collections.Generic;

namespace Masterdom.Platform.Events;

/// <summary>
/// Default event store implementation backed by an event repository.
/// </summary>
public sealed class EventStore : IEventStore
{
    private readonly IEventRepository _repository;

    public EventStore(IEventRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public void Append(EventEnvelope envelope)
    {
        _repository.Save(envelope);
    }

    public IReadOnlyList<EventEnvelope> Read(EventReadRequest request)
    {
        return _repository.Read(request);
    }
}
