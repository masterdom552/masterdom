using System;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Represents the scope of a metadata definition.
/// </summary>
public sealed class MetadataScope : IEquatable<MetadataScope>
{
    private MetadataScope(MetadataScopeKind kind, string? identifier)
    {
        Kind = kind;
        Identifier = identifier;
    }

    public MetadataScopeKind Kind { get; }

    public string? Identifier { get; }

    public static MetadataScope Global()
    {
        return new MetadataScope(MetadataScopeKind.Global, null);
    }

    public static MetadataScope Module(string moduleId)
    {
        return Create(MetadataScopeKind.Module, moduleId);
    }

    public static MetadataScope Aggregate(string aggregateName)
    {
        return Create(MetadataScopeKind.Aggregate, aggregateName);
    }

    public static MetadataScope Entity(string entityName)
    {
        return Create(MetadataScopeKind.Entity, entityName);
    }

    public static MetadataScope Property(string propertyName)
    {
        return Create(MetadataScopeKind.Property, propertyName);
    }

    public static MetadataScope Field(string fieldName)
    {
        return Create(MetadataScopeKind.Field, fieldName);
    }

    public static MetadataScope Enumeration(string enumName)
    {
        return Create(MetadataScopeKind.Enumeration, enumName);
    }

    public static MetadataScope Create(MetadataScopeKind kind, string? identifier)
    {
        if (kind == MetadataScopeKind.Global)
        {
            if (!string.IsNullOrWhiteSpace(identifier))
            {
                throw new MetadataValidationException(
                    "Global metadata scope cannot contain an identifier.");
            }

            return Global();
        }

        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new MetadataValidationException(
                $"Metadata scope identifier is required for scope '{kind}'.");
        }

        return new MetadataScope(kind, identifier.Trim());
    }

    public bool Equals(MetadataScope? other)
    {
        if (other is null)
        {
            return false;
        }

        return Kind == other.Kind &&
               string.Equals(Identifier, other.Identifier, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is MetadataScope other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Kind,
            Identifier?.ToUpperInvariant());
    }

    public override string ToString()
    {
        return Identifier is null
            ? Kind.ToString()
            : $"{Kind}:{Identifier}";
    }
}
