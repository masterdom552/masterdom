using System.Collections.Generic;

namespace Masterdom.Platform.Events;

/// <summary>
/// Provides append and query operations for persisted event envelopes.
/// </summary>
public interface IEventRepository
{
    void Save(EventEnvelope envelope);

    IReadOnlyList<EventEnvelope> Read(EventReadRequest request);
}
