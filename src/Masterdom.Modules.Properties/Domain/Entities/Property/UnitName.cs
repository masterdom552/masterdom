using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Properties.Domain.Entities.Property;

/// <summary>
/// Represents the display name of a unit.
/// </summary>
public sealed class UnitName : ValueObject
{
    public UnitName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value.Trim();
    }

    public string Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;

    public static implicit operator string(UnitName name)
        => name.Value;
}
