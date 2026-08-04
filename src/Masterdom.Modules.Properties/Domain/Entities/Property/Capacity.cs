using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Properties.Domain.Entities.Property;

/// <summary>
/// Represents maximum occupancy capacity for a unit.
/// </summary>
public sealed class Capacity : ValueObject
{
    public Capacity(int value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Capacity must be greater than zero.");
        }

        Value = value;
    }

    public int Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value.ToString();
}
