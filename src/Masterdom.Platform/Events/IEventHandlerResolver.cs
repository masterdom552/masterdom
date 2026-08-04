using System.Collections.Generic;

namespace Masterdom.Platform.Events;

/// <summary>
/// Resolves handlers for a specific event envelope.
/// </summary>
public interface IEventHandlerResolver
{
    IReadOnlyList<IEventHandler> Resolve(EventEnvelope envelope, EventDispatchPolicy policy);
}
