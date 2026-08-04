using System;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Represents the unique identity of a metadata definition.
/// </summary>
public readonly struct MetadataId
{
    public MetadataId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new MetadataValidationException(
                "MetadataId cannot be empty.");
        }

        Value = value;
    }

    public Guid Value { get; }
}
