using System;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents a versioned rule definition.
/// </summary>
public sealed class RuleDefinition
{
    public RuleDefinition(
        RuleId id,
        RuleSetId ruleSetId,
        RuleKey key,
        string name,
        string? description,
        RuleKind kind,
        RuleCondition condition,
        RuleCategory category,
        RuleSeverity severity,
        RulePriority priority,
        RuleScope scope,
        RuleVersion version,
        RuleEffectivePeriod period,
        RuleId? parentRuleId,
        bool isDeprecated,
        RuleKey? replacedByKey,
        string? compatibility,
        string changedBy,
        DateTime changedAtUtc)
    {
        Id = id;
        RuleSetId = ruleSetId;
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new RuleValidationException("Rule name is required.");
        }

        if (string.IsNullOrWhiteSpace(changedBy))
        {
            throw new RuleValidationException("ChangedBy is required for rules.");
        }

        if (changedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new RuleValidationException("ChangedAtUtc must be UTC for rules.");
        }

        if (isDeprecated && replacedByKey is null)
        {
            throw new RuleValidationException(
                "Deprecated rules must declare a replacement key.");
        }

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Kind = kind;
        Category = category;
        Severity = severity;
        Priority = priority;
        Version = version;
        Period = period;
        ParentRuleId = parentRuleId;
        IsDeprecated = isDeprecated;
        ReplacedByKey = replacedByKey;
        Compatibility = string.IsNullOrWhiteSpace(compatibility) ? null : compatibility.Trim();
        ChangedBy = changedBy.Trim();
        ChangedAtUtc = changedAtUtc;
    }

    public RuleId Id { get; }

    public RuleSetId RuleSetId { get; }

    public RuleKey Key { get; }

    public string Name { get; }

    public string? Description { get; }

    public RuleKind Kind { get; }

    public RuleCondition Condition { get; }

    public RuleCategory Category { get; }

    public RuleSeverity Severity { get; }

    public RulePriority Priority { get; }

    public RuleScope Scope { get; }

    public RuleVersion Version { get; }

    public RuleEffectivePeriod Period { get; }

    public RuleId? ParentRuleId { get; }

    public bool IsDeprecated { get; }

    public RuleKey? ReplacedByKey { get; }

    public string? Compatibility { get; }

    public string ChangedBy { get; }

    public DateTime ChangedAtUtc { get; }
}
