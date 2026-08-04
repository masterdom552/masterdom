namespace Masterdom.Platform.Events;

/// <summary>
/// Represents output of publish operation.
/// </summary>
public sealed class EventPublishResult
{
    public required EventEnvelope Envelope { get; init; }

    public required EventDispatchResult Dispatch { get; init; }
}
