using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class RatingSnapshot : ValueObject
{
    private RatingSnapshot(
        ConsumptionSnapshot consumptionSnapshot,
        TariffSchedule tariffSchedule,
        DateTime capturedAtUtc)
    {
        ConsumptionSnapshot = consumptionSnapshot;
        TariffSchedule = tariffSchedule;
        CapturedAtUtc = capturedAtUtc;
    }

    public ConsumptionSnapshot ConsumptionSnapshot { get; }

    public TariffSchedule TariffSchedule { get; }

    public DateTime CapturedAtUtc { get; }

    public static RatingSnapshot Create(
        ConsumptionSnapshot consumptionSnapshot,
        TariffSchedule tariffSchedule,
        DateTime capturedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(consumptionSnapshot);
        ArgumentNullException.ThrowIfNull(tariffSchedule);

        if (capturedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Rating snapshot timestamp must be UTC.");
        }

        return new RatingSnapshot(consumptionSnapshot, tariffSchedule, capturedAtUtc);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ConsumptionSnapshot;
        yield return TariffSchedule;
        yield return CapturedAtUtc;
    }
}
