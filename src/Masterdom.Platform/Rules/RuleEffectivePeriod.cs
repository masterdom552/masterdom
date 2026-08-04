using System;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents effective dates for rule and rule-set versions.
/// </summary>
public readonly struct RuleEffectivePeriod
{
    public RuleEffectivePeriod(DateTime effectiveFromUtc, DateTime? effectiveToUtc)
    {
        if (effectiveFromUtc.Kind != DateTimeKind.Utc)
        {
            throw new RuleValidationException("Rule effective-from timestamp must be UTC.");
        }

        if (effectiveToUtc.HasValue && effectiveToUtc.Value.Kind != DateTimeKind.Utc)
        {
            throw new RuleValidationException("Rule effective-to timestamp must be UTC.");
        }

        if (effectiveToUtc.HasValue && effectiveToUtc.Value <= effectiveFromUtc)
        {
            throw new RuleValidationException(
                "Rule effective-to timestamp must be greater than effective-from.");
        }

        EffectiveFromUtc = effectiveFromUtc;
        EffectiveToUtc = effectiveToUtc;
    }

    public DateTime EffectiveFromUtc { get; }

    public DateTime? EffectiveToUtc { get; }

    public bool IsEffectiveAt(DateTime asOfUtc)
    {
        if (asOfUtc.Kind != DateTimeKind.Utc)
        {
            throw new RuleValidationException("Rule evaluation timestamp must be UTC.");
        }

        if (asOfUtc < EffectiveFromUtc)
        {
            return false;
        }

        return !EffectiveToUtc.HasValue || asOfUtc < EffectiveToUtc.Value;
    }
}
