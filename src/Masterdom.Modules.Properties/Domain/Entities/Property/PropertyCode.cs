using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Properties.Domain.Entities.Property;

/// <summary>
/// Represents the unique business code assigned to a property.
///
/// Examples:
/// 47
/// 85
/// SHOP-09
/// SHOP-MARKET
/// WAREHOUSE
/// TOWERS
/// </summary>
public sealed class PropertyCode : ValueObject
{
    public PropertyCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Gets the normalized property code.
    /// </summary>
    public string Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;

    public static implicit operator string(PropertyCode code)
        => code.Value;
}
