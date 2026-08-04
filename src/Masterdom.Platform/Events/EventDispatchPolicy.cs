namespace Masterdom.Platform.Events;

/// <summary>
/// Defines policy controls for event dispatch execution.
/// </summary>
public sealed class EventDispatchPolicy
{
    public bool ContinueOnHandlerFailure { get; init; } = true;

    public EventDispatchOrdering Ordering { get; init; } = EventDispatchOrdering.RegistrationOrder;

    public bool RequireAtLeastOneHandler { get; init; }

    public bool CaptureDiagnostics { get; init; } = true;
}
