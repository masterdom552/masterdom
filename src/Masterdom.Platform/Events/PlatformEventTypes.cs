namespace Masterdom.Platform.Events;

/// <summary>
/// Provides canonical platform lifecycle event type names.
/// </summary>
public static class PlatformEventTypes
{
    public static EventType KernelStarted { get; } =
        new EventType("platform.kernel.started");

    public static EventType KernelStopped { get; } =
        new EventType("platform.kernel.stopped");

    public static EventType ModuleLoaded(string moduleId)
    {
        return new EventType($"platform.module.{moduleId}.loaded");
    }

    public static EventType ModuleInitialized(string moduleId)
    {
        return new EventType($"platform.module.{moduleId}.initialized");
    }

    public static EventType ModuleShutdown(string moduleId)
    {
        return new EventType($"platform.module.{moduleId}.shutdown");
    }
}
