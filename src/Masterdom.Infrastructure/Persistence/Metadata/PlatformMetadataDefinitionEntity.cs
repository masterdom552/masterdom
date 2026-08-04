using System;

namespace Masterdom.Infrastructure.Persistence.Metadata;

/// <summary>
/// Persistence model for platform metadata definitions.
/// </summary>
public sealed class PlatformMetadataDefinitionEntity
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public int Category { get; set; }

    public int ScopeKind { get; set; }

    public string? ScopeIdentifier { get; set; }

    public int Version { get; set; }

    public DateTime EffectiveFromUtc { get; set; }

    public DateTime? EffectiveToUtc { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? ParentId { get; set; }

    public bool IsDeprecated { get; set; }

    public string? ReplacedByKey { get; set; }

    public string? Compatibility { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAtUtc { get; set; }
}
