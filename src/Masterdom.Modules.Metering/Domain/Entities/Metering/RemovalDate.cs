using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class RemovalDate : ValueObject
{
    private RemovalDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static RemovalDate Create(DateOnly value)
    {
        return new RemovalDate(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
