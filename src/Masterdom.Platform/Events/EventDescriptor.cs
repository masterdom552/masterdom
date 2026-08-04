namespace Masterdom.Platform.Events;

/// <summary>
/// Represents a registered event type descriptor.
/// </summary>
public sealed class EventDescriptor
{
    public required EventType EventType { get; init; }

    public required EventCategory Category { get; init; }

    public required EventVersion Version { get; init; }

    public bool RequiresHandler { get; init; }
}
