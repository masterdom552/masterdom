using System.Collections;

namespace Masterdom.Core.Primitives;

/// <summary>
/// Represents an immutable value object.
///
/// Equality is determined by comparing all equality components
/// supplied by the derived type.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Returns the values that participate in equality.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj)
        => Equals(obj as ValueObject);

    public override int GetHashCode()
    {
        HashCode hash = new();

        foreach (object? component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(
        ValueObject? left,
        ValueObject? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(
        ValueObject? left,
        ValueObject? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Utility helper for collections inside value objects.
    /// </summary>
    protected static IEnumerable<object?> GetEnumerableComponents<T>(
        IEnumerable<T> values)
    {
        foreach (T value in values)
            yield return value;
    }
}
