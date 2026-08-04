using System.Collections.Generic;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents output of publishing one aggregate domain-event batch.
/// </summary>
public sealed class DomainEventPublishResult
{
    public required int PublishedCount { get; init; }

    public required IReadOnlyList<EventPublishResult> PublishedEvents { get; init; }
}
