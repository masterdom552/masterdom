using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents effective dates for workflow versions.
/// </summary>
public readonly struct WorkflowEffectivePeriod
{
    public WorkflowEffectivePeriod(DateTime effectiveFromUtc, DateTime? effectiveToUtc)
    {
        if (effectiveFromUtc.Kind != DateTimeKind.Utc)
        {
            throw new WorkflowValidationException("Workflow effective-from timestamp must be UTC.");
        }

        if (effectiveToUtc.HasValue && effectiveToUtc.Value.Kind != DateTimeKind.Utc)
        {
            throw new WorkflowValidationException("Workflow effective-to timestamp must be UTC.");
        }

        if (effectiveToUtc.HasValue && effectiveToUtc.Value <= effectiveFromUtc)
        {
            throw new WorkflowValidationException("Workflow effective-to timestamp must be greater than effective-from.");
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
            throw new WorkflowValidationException("Workflow evaluation timestamp must be UTC.");
        }

        if (asOfUtc < EffectiveFromUtc)
        {
            return false;
        }

        return !EffectiveToUtc.HasValue || asOfUtc < EffectiveToUtc.Value;
    }
}
