using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class VariableCharge : ValueObject
{
    private VariableCharge(decimal ratePerUnit)
    {
        RatePerUnit = decimal.Round(ratePerUnit, 4, MidpointRounding.AwayFromZero);
    }

    public decimal RatePerUnit { get; }

    public static VariableCharge Create(decimal ratePerUnit)
    {
        if (ratePerUnit < 0)
        {
            throw new InvalidOperationException("Variable rate per unit cannot be negative.");
        }

        return new VariableCharge(ratePerUnit);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return RatePerUnit;
    }
}
