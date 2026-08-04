using System;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Represents the effective period of a metadata definition version.
/// </summary>
public readonly struct MetadataEffectivePeriod
{
    public MetadataEffectivePeriod(DateTime effectiveFromUtc, DateTime? effectiveToUtc)
    {
        if (effectiveFromUtc.Kind != DateTimeKind.Utc)
        {
            throw new MetadataValidationException(
                "Metadata effective-from timestamp must be UTC.");
        }

        if (effectiveToUtc.HasValue && effectiveToUtc.Value.Kind != DateTimeKind.Utc)
        {
            throw new MetadataValidationException(
                "Metadata effective-to timestamp must be UTC.");
        }

        if (effectiveToUtc.HasValue && effectiveToUtc.Value <= effectiveFromUtc)
        {
            throw new MetadataValidationException(
                "Metadata effective-to timestamp must be greater than effective-from.");
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
            throw new MetadataValidationException(
                "Metadata resolution timestamp must be UTC.");
        }

        if (asOfUtc < EffectiveFromUtc)
        {
            return false;
        }

        return !EffectiveToUtc.HasValue || asOfUtc < EffectiveToUtc.Value;
    }
}
