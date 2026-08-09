using Masterdom.Modules.UtilityRating.Contracts.Metering;
using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

namespace Masterdom.Modules.UtilityRating.Application.Commands;

/// <summary>
/// Requests rating for a metering consumption output using a governed tariff.
/// </summary>
/// <param name="ConsumptionOutput">The metering consumption to rate.</param>
/// <param name="TariffCode">The governed tariff code to resolve.</param>
public sealed record RateConsumptionCommand(
    MeteringConsumptionOutputContract ConsumptionOutput,
    string TariffCode)
{
    /// <summary>
    /// Creates the domain consumption snapshot.
    /// </summary>
    public ConsumptionSnapshot ToSnapshot()
    {
        return ConsumptionSnapshot.Create(
            MeterReference.Create(ConsumptionOutput.MeterId),
            ConsumptionReference.Create(ConsumptionOutput.ReadingId, ConsumptionOutput.ConsumptionValue),
            RatingPeriod.Create(ConsumptionOutput.PeriodStart, ConsumptionOutput.PeriodEnd),
            ConsumptionOutput.CapturedAtUtc);
    }
}
