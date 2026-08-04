using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class Consumption : ValueObject
{
    private Consumption(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static Consumption Create(decimal value)
    {
        if (value < 0)
        {
            throw new InvalidOperationException("Consumption cannot be negative.");
        }

        return new Consumption(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
