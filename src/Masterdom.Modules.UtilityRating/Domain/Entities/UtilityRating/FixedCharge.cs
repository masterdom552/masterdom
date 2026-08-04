namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class FixedCharge : RateComponent
{
    private FixedCharge(decimal amount)
        : base("FixedCharge", amount)
    {
    }

    public static FixedCharge Create(decimal amount)
    {
        return new FixedCharge(amount);
    }
}
