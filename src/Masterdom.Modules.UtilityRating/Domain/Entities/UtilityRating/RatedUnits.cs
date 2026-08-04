using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class RatedUnits : ValueObject
{
    private RatedUnits(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static RatedUnits Create(decimal value)
    {
        if (value < 0)
        {
            throw new InvalidOperationException("Rated units cannot be negative.");
        }

        return new RatedUnits(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
