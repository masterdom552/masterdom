using System;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Represents an effective period for a configuration version.
/// </summary>
public readonly struct EffectivePeriod
{
    public EffectivePeriod(DateTime effectiveFromUtc, DateTime? effectiveToUtc)
    {
        if (effectiveFromUtc.Kind != DateTimeKind.Utc)
        {
            throw new PlatformConfigurationValidationException(
                "EffectiveFromUtc must be specified in UTC.");
        }

        if (effectiveToUtc.HasValue && effectiveToUtc.Value.Kind != DateTimeKind.Utc)
        {
            throw new PlatformConfigurationValidationException(
                "EffectiveToUtc must be specified in UTC when provided.");
        }

        if (effectiveToUtc.HasValue && effectiveToUtc.Value <= effectiveFromUtc)
        {
            throw new PlatformConfigurationValidationException(
                "EffectiveToUtc must be greater than EffectiveFromUtc.");
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
            throw new PlatformConfigurationValidationException(
                "Resolution timestamp must be UTC.");
        }

        if (asOfUtc < EffectiveFromUtc)
        {
            return false;
        }

        return !EffectiveToUtc.HasValue || asOfUtc < EffectiveToUtc.Value;
    }
}
