namespace Masterdom.Platform.Events;

/// <summary>
/// Represents one diagnostic entry generated during dispatch.
/// </summary>
public sealed class EventDispatchDiagnostic
{
    public required EventDispatchDiagnosticSeverity Severity { get; init; }

    public required string Message { get; init; }

    public string? HandlerId { get; init; }
}
