using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class ReadingDate : ValueObject
{
    private ReadingDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static ReadingDate Create(DateOnly value)
    {
        return new ReadingDate(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
