using Masterdom.Modules.UtilityRating.Contracts.Metering;
using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

namespace Masterdom.Modules.UtilityRating.Application.Commands;

public sealed record RateConsumptionCommand(
    MeteringConsumptionOutputContract ConsumptionOutput,
    TariffSchedule TariffSchedule)
{
    public ConsumptionSnapshot ToSnapshot()
    {
        return ConsumptionSnapshot.Create(
            MeterReference.Create(ConsumptionOutput.MeterId),
            ConsumptionReference.Create(ConsumptionOutput.ReadingId, ConsumptionOutput.ConsumptionValue),
            RatingPeriod.Create(ConsumptionOutput.PeriodStart, ConsumptionOutput.PeriodEnd),
            ConsumptionOutput.CapturedAtUtc);
    }
}
