using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class ReadingValue : ValueObject
{
    private ReadingValue(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static ReadingValue Create(decimal value)
    {
        if (value < 0)
        {
            throw new InvalidOperationException("Reading value cannot be negative.");
        }

        return new ReadingValue(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
