namespace Masterdom.Platform.Events;

/// <summary>
/// Defines ordering strategies for dispatch execution.
/// </summary>
public enum EventDispatchOrdering
{
    RegistrationOrder = 0,
    ExplicitOrder = 1
}
