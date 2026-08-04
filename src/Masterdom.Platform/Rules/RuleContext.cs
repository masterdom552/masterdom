using System;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents runtime context used during rule evaluation.
/// </summary>
public sealed class RuleContext
{
    public required string ModuleId { get; init; }

    public string? TenantId { get; init; }

    public string? PropertyId { get; init; }

    public DateTime AsOfUtc { get; init; }

    public string? CorrelationId { get; init; }
}
