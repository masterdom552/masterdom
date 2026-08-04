using System;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Represents an immutable versioned configuration record.
/// </summary>
public sealed class ConfigurationRecord
{
    public ConfigurationRecord(
        ConfigurationId id,
        ConfigurationKey key,
        ConfigurationScope scope,
        ConfigurationVersion version,
        ConfigurationValue value,
        EffectivePeriod period,
        string changedBy,
        string reason,
        DateTime changedAtUtc)
    {
        Id = id;
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        Value = value ?? throw new ArgumentNullException(nameof(value));

        if (string.IsNullOrWhiteSpace(changedBy))
        {
            throw new PlatformConfigurationValidationException(
                "ChangedBy is required.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new PlatformConfigurationValidationException(
                "Reason is required.");
        }

        if (changedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new PlatformConfigurationValidationException(
                "ChangedAtUtc must be UTC.");
        }

        Version = version;
        Period = period;
        ChangedBy = changedBy.Trim();
        Reason = reason.Trim();
        ChangedAtUtc = changedAtUtc;
    }

    public ConfigurationId Id { get; }

    public ConfigurationKey Key { get; }

    public ConfigurationScope Scope { get; }

    public ConfigurationVersion Version { get; }

    public ConfigurationValue Value { get; }

    public EffectivePeriod Period { get; }

    public string ChangedBy { get; }

    public string Reason { get; }

    public DateTime ChangedAtUtc { get; }
}
