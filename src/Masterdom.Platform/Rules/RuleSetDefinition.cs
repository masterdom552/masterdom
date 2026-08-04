using System;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents a versioned rule set definition.
/// </summary>
public sealed class RuleSetDefinition
{
    public RuleSetDefinition(
        RuleSetId id,
        RuleSetKey key,
        string name,
        string? description,
        RuleCategory category,
        RuleScope scope,
        RuleVersion version,
        RuleEffectivePeriod period,
        bool isDeprecated,
        RuleSetKey? replacedByKey,
        string? compatibility,
        string changedBy,
        DateTime changedAtUtc)
    {
        Id = id;
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new RuleValidationException("Rule set name is required.");
        }

        if (string.IsNullOrWhiteSpace(changedBy))
        {
            throw new RuleValidationException("ChangedBy is required for rule sets.");
        }

        if (changedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new RuleValidationException("ChangedAtUtc must be UTC for rule sets.");
        }

        if (isDeprecated && replacedByKey is null)
        {
            throw new RuleValidationException(
                "Deprecated rule sets must declare a replacement key.");
        }

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Category = category;
        Version = version;
        Period = period;
        IsDeprecated = isDeprecated;
        ReplacedByKey = replacedByKey;
        Compatibility = string.IsNullOrWhiteSpace(compatibility) ? null : compatibility.Trim();
        ChangedBy = changedBy.Trim();
        ChangedAtUtc = changedAtUtc;
    }

    public RuleSetId Id { get; }

    public RuleSetKey Key { get; }

    public string Name { get; }

    public string? Description { get; }

    public RuleCategory Category { get; }

    public RuleScope Scope { get; }

    public RuleVersion Version { get; }

    public RuleEffectivePeriod Period { get; }

    public bool IsDeprecated { get; }

    public RuleSetKey? ReplacedByKey { get; }

    public string? Compatibility { get; }

    public string ChangedBy { get; }

    public DateTime ChangedAtUtc { get; }
}
