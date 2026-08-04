using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class RatedAmount : ValueObject
{
    private RatedAmount(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static RatedAmount Create(decimal value)
    {
        if (value < 0)
        {
            throw new InvalidOperationException("Rated amount cannot be negative.");
        }

        return new RatedAmount(decimal.Round(value, 2, MidpointRounding.AwayFromZero));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
