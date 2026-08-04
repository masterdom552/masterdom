using System;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents context used to publish and dispatch an event.
/// </summary>
public sealed class EventContext
{
    public required string ModuleId { get; init; }

    public string? CorrelationId { get; init; }

    public string? CausationId { get; init; }

    public string? TenantId { get; init; }

    public string? AggregateId { get; init; }

    public string? AggregateType { get; init; }

    public DateTime OccurredAtUtc { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ModuleId))
        {
            throw new EventValidationException("ModuleId is required for event context.");
        }

        if (OccurredAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new EventValidationException("OccurredAtUtc must be UTC.");
        }
    }
}
