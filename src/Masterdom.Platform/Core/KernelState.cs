namespace Masterdom.Platform.Core;

/// <summary>
/// Represents the lifecycle state of the platform kernel.
/// </summary>
public enum KernelState
{
    /// <summary>
    /// The kernel has been created but has not started.
    /// Modules may still be loaded.
    /// </summary>
    Created,

    /// <summary>
    /// The kernel is starting.
    /// Modules are being registered and initialized.
    /// </summary>
    Starting,

    /// <summary>
    /// The kernel is fully started and running.
    /// </summary>
    Running,

    /// <summary>
    /// The kernel encountered an unrecoverable error during startup
    /// or execution and is no longer in a valid operational state.
    /// </summary>
    Faulted,

    /// <summary>
    /// The kernel is shutting down.
    /// </summary>
    Stopping,

    /// <summary>
    /// The kernel has been shut down.
    /// </summary>
    Stopped
}