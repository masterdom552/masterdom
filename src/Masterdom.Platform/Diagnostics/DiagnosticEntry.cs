using System;

namespace Masterdom.Platform.Diagnostics;

/// <summary>
/// Represents a single diagnostic event.
/// </summary>
public sealed class DiagnosticEntry
{
    /// <summary>
    /// Gets the UTC timestamp.
    /// </summary>
    public required DateTime TimestampUtc { get; init; }

    /// <summary>
    /// Gets the severity.
    /// </summary>
    public required DiagnosticSeverity Severity { get; init; }

    /// <summary>
    /// Gets the component generating the event.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Gets the message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the associated exception.
    /// </summary>
    public Exception? Exception { get; init; }
}