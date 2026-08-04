using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class MinimumCharge : ValueObject
{
    private MinimumCharge(decimal amount)
    {
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    public decimal Amount { get; }

    public static MinimumCharge Create(decimal amount)
    {
        if (amount < 0)
        {
            throw new InvalidOperationException("Minimum charge cannot be negative.");
        }

        return new MinimumCharge(amount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }
}
