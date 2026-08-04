using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Properties.Domain.Entities.Property;


/// <summary>
/// Represents the display name of a property.
/// </summary>
public sealed class PropertyName : ValueObject
{
    public PropertyName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value.Trim();
    }

    /// <summary>
    /// Gets the normalized property name.
    /// </summary>
    public string Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;

    public static implicit operator string(PropertyName name)
        => name.Value;
}
