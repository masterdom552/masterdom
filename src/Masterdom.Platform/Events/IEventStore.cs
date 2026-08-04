using System.Collections.Generic;

namespace Masterdom.Platform.Events;

/// <summary>
/// Captures and reads event envelopes for replay-ready infrastructure.
/// </summary>
public interface IEventStore
{
    void Append(EventEnvelope envelope);

    IReadOnlyList<EventEnvelope> Read(EventReadRequest request);
}
