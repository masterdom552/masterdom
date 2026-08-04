using System;

namespace Masterdom.Platform.Core;

/// <summary>
/// Evaluates health for the current kernel lifecycle state.
/// </summary>
public sealed class KernelHealthCheck
{
    /// <summary>
    /// Evaluates the specified kernel state.
    /// </summary>
    public KernelHealthCheckResult Evaluate(KernelState state)
    {
        return new KernelHealthCheckResult
        {
            TimestampUtc = DateTime.UtcNow,
            State = state,
            Status = MapStatus(state),
            Message = MapMessage(state)
        };
    }

    private static KernelHealthStatus MapStatus(KernelState state)
    {
        return state switch
        {
            KernelState.Running => KernelHealthStatus.Healthy,
            KernelState.Starting or KernelState.Stopping => KernelHealthStatus.Degraded,
            KernelState.Created => KernelHealthStatus.Degraded,
            KernelState.Stopped => KernelHealthStatus.Degraded,
            KernelState.Faulted => KernelHealthStatus.Unhealthy,
            _ => KernelHealthStatus.Unhealthy
        };
    }

    private static string MapMessage(KernelState state)
    {
        return state switch
        {
            KernelState.Created => "Kernel is created and waiting to start.",
            KernelState.Starting => "Kernel startup is in progress.",
            KernelState.Running => "Kernel is running.",
            KernelState.Stopping => "Kernel shutdown is in progress.",
            KernelState.Stopped => "Kernel is stopped.",
            KernelState.Faulted => "Kernel is faulted.",
            _ => "Kernel state is unknown."
        };
    }
}
