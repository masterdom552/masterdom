using System;

namespace Masterdom.Infrastructure.Persistence.Configuration;

/// <summary>
/// Persistence model for versioned platform configuration records.
/// </summary>
public sealed class PlatformConfigurationRecordEntity
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public int ScopeKind { get; set; }

    public string? ScopeIdentifier { get; set; }

    public int Version { get; set; }

    public string Value { get; set; } = string.Empty;

    public DateTime EffectiveFromUtc { get; set; }

    public DateTime? EffectiveToUtc { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public DateTime ChangedAtUtc { get; set; }
}
