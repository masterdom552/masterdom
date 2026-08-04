using System;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Represents a normalized metadata key.
/// </summary>
public sealed class MetadataKey : IEquatable<MetadataKey>
{
    public MetadataKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new MetadataValidationException(
                "Metadata key is required.");
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public bool Equals(MetadataKey? other)
    {
        if (other is null)
        {
            return false;
        }

        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is MetadataKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.ToUpperInvariant().GetHashCode(StringComparison.Ordinal);
    }

    public override string ToString()
    {
        return Value;
    }
}
