using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class RatedConsumption : ValueObject
{
    private RatedConsumption(RatedUnits ratedUnits, RatedAmount ratedAmount)
    {
        RatedUnits = ratedUnits;
        RatedAmount = ratedAmount;
    }

    public RatedUnits RatedUnits { get; }

    public RatedAmount RatedAmount { get; }

    public static RatedConsumption Create(RatedUnits ratedUnits, RatedAmount ratedAmount)
    {
        ArgumentNullException.ThrowIfNull(ratedUnits);
        ArgumentNullException.ThrowIfNull(ratedAmount);

        return new RatedConsumption(ratedUnits, ratedAmount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return RatedUnits;
        yield return RatedAmount;
    }
}
