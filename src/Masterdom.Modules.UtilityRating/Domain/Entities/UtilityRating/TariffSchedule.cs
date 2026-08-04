using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class TariffSchedule : ValueObject
{
    private TariffSchedule(
        TariffReference tariffReference,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        UtilityRate utilityRate)
    {
        TariffReference = tariffReference;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        UtilityRate = utilityRate;
    }

    public TariffReference TariffReference { get; }

    public DateOnly EffectiveFrom { get; }

    public DateOnly? EffectiveTo { get; }

    public UtilityRate UtilityRate { get; }

    public static TariffSchedule Create(
        TariffReference tariffReference,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        UtilityRate utilityRate)
    {
        ArgumentNullException.ThrowIfNull(tariffReference);
        ArgumentNullException.ThrowIfNull(utilityRate);

        if (effectiveTo.HasValue && effectiveTo.Value <= effectiveFrom)
        {
            throw new InvalidOperationException("Tariff schedule end date must be after start date.");
        }

        return new TariffSchedule(tariffReference, effectiveFrom, effectiveTo, utilityRate);
    }

    public void EnsureCovers(RatingPeriod period)
    {
        ArgumentNullException.ThrowIfNull(period);

        if (period.StartDate < EffectiveFrom)
        {
            throw new InvalidOperationException("Tariff schedule is not effective for rating period start date.");
        }

        if (EffectiveTo.HasValue && period.EndDate > EffectiveTo.Value)
        {
            throw new InvalidOperationException("Tariff schedule is not effective for rating period end date.");
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return TariffReference;
        yield return EffectiveFrom;
        yield return EffectiveTo;
        yield return UtilityRate;
    }
}
