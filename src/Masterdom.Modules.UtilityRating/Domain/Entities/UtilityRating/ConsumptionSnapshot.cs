using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class ConsumptionSnapshot : ValueObject
{
    private ConsumptionSnapshot(
        MeterReference meterReference,
        ConsumptionReference consumptionReference,
        RatingPeriod ratingPeriod,
        DateTime capturedAtUtc)
    {
        MeterReference = meterReference;
        ConsumptionReference = consumptionReference;
        RatingPeriod = ratingPeriod;
        CapturedAtUtc = capturedAtUtc;
    }

    public MeterReference MeterReference { get; }

    public ConsumptionReference ConsumptionReference { get; }

    public RatingPeriod RatingPeriod { get; }

    public DateTime CapturedAtUtc { get; }

    public static ConsumptionSnapshot Create(
        MeterReference meterReference,
        ConsumptionReference consumptionReference,
        RatingPeriod ratingPeriod,
        DateTime capturedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(meterReference);
        ArgumentNullException.ThrowIfNull(consumptionReference);
        ArgumentNullException.ThrowIfNull(ratingPeriod);

        if (capturedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Consumption snapshot timestamp must be UTC.");
        }

        return new ConsumptionSnapshot(meterReference, consumptionReference, ratingPeriod, capturedAtUtc);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MeterReference;
        yield return ConsumptionReference;
        yield return RatingPeriod;
        yield return CapturedAtUtc;
    }
}
