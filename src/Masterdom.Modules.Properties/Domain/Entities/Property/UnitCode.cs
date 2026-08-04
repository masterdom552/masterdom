using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Properties.Domain.Entities.Property;

/// <summary>
/// Represents a unit code within a property.
/// </summary>
public sealed class UnitCode : ValueObject
{
    public UnitCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Gets the normalized unit code.
    /// </summary>
    public string Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;

    public static implicit operator string(UnitCode code)
        => code.Value;
}
