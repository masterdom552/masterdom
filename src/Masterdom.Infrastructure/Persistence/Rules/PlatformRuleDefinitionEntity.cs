using System;

namespace Masterdom.Infrastructure.Persistence.Rules;

/// <summary>
/// Persistence model for versioned platform rules.
/// </summary>
public sealed class PlatformRuleDefinitionEntity
{
    public Guid Id { get; set; }

    public Guid RuleSetId { get; set; }

    public Guid? ParentRuleId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Kind { get; set; }

    public int Category { get; set; }

    public int Severity { get; set; }

    public int Priority { get; set; }

    public int ScopeKind { get; set; }

    public string? ScopeIdentifier { get; set; }

    public int Version { get; set; }

    public DateTime EffectiveFromUtc { get; set; }

    public DateTime? EffectiveToUtc { get; set; }

    public bool IsDeprecated { get; set; }

    public string? ReplacedByKey { get; set; }

    public string? Compatibility { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAtUtc { get; set; }

    public string? InputKey { get; set; }

    public int? ComparisonOperator { get; set; }

    public string? CompareInputKey { get; set; }

    public int? ExpectedValueKind { get; set; }

    public bool? ExpectedBoolean { get; set; }

    public decimal? ExpectedNumber { get; set; }

    public string? ExpectedText { get; set; }

    public decimal? MinNumber { get; set; }

    public decimal? MaxNumber { get; set; }

    public int? CompositeOperator { get; set; }

    public int? ArithmeticOperator { get; set; }

    public string? ExpressionLeftKey { get; set; }

    public string? ExpressionRightKey { get; set; }

    public decimal? ExpressionExpectedNumber { get; set; }
}
