namespace Masterdom.Platform.Events;

/// <summary>
/// Defines event categories supported by the platform event hierarchy.
/// </summary>
public enum EventCategory
{
    Domain = 0,
    Platform = 1,
    Application = 2,
    Integration = 3,
    Notification = 4,
    System = 5,
    Lifecycle = 6
}
