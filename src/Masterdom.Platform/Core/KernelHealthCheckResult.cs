using System;

namespace Masterdom.Platform.Core;

/// <summary>
/// Represents kernel health check output.
/// </summary>
public sealed class KernelHealthCheckResult
{
    public required DateTime TimestampUtc { get; init; }

    public required KernelHealthStatus Status { get; init; }

    public required KernelState State { get; init; }

    public required string Message { get; init; }
}
