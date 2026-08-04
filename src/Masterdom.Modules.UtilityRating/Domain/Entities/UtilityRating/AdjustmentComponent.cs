namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class AdjustmentComponent : RateComponent
{
    private AdjustmentComponent(decimal amount)
        : base("AdjustmentComponent", amount)
    {
    }

    public static AdjustmentComponent Create(decimal amount)
    {
        return new AdjustmentComponent(amount);
    }
}
