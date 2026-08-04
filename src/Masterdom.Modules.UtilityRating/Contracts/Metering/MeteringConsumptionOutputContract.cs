namespace Masterdom.Modules.UtilityRating.Contracts.Metering;

public sealed record MeteringConsumptionOutputContract(
    Guid MeterId,
    Guid ReadingId,
    decimal ConsumptionValue,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateTime CapturedAtUtc);
