using System.Collections.Generic;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents an immutable snapshot of event and subscription metadata.
/// </summary>
public interface IEventCatalog
{
    IReadOnlyList<EventDescriptor> Events { get; }

    IReadOnlyList<EventHandlerDescriptor> Handlers { get; }
}
