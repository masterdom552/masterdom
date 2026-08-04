using System;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Represents an immutable metadata definition version.
/// </summary>
public sealed class MetadataDefinition
{
    public MetadataDefinition(
        MetadataId id,
        MetadataKey key,
        MetadataCategory category,
        MetadataScope scope,
        MetadataVersion version,
        MetadataEffectivePeriod period,
        string name,
        string? description,
        MetadataId? parentId,
        bool isDeprecated,
        MetadataKey? replacedByKey,
        string? compatibility,
        string changedBy,
        DateTime changedAtUtc)
    {
        Id = id;
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new MetadataValidationException("Metadata name is required.");
        }

        if (string.IsNullOrWhiteSpace(changedBy))
        {
            throw new MetadataValidationException("ChangedBy is required for metadata definition.");
        }

        if (changedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new MetadataValidationException("ChangedAtUtc must be UTC for metadata definition.");
        }

        if (isDeprecated && replacedByKey is null)
        {
            throw new MetadataValidationException(
                "Deprecated metadata definitions must declare a replacement key.");
        }

        Category = category;
        Version = version;
        Period = period;
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ParentId = parentId;
        IsDeprecated = isDeprecated;
        ReplacedByKey = replacedByKey;
        Compatibility = string.IsNullOrWhiteSpace(compatibility) ? null : compatibility.Trim();
        ChangedBy = changedBy.Trim();
        ChangedAtUtc = changedAtUtc;
    }

    public MetadataId Id { get; }

    public MetadataKey Key { get; }

    public MetadataCategory Category { get; }

    public MetadataScope Scope { get; }

    public MetadataVersion Version { get; }

    public MetadataEffectivePeriod Period { get; }

    public string Name { get; }

    public string? Description { get; }

    public MetadataId? ParentId { get; }

    public bool IsDeprecated { get; }

    public MetadataKey? ReplacedByKey { get; }

    public string? Compatibility { get; }

    public string ChangedBy { get; }

    public DateTime ChangedAtUtc { get; }
}
