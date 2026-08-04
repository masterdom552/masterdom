namespace Masterdom.Modules.SubsidyOptimization.Contracts.UtilityRating;

public sealed record RatedConsumptionContract(
    Guid RatingId,
    Guid MeterId,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal RatedUnits,
    decimal RatedAmount,
    DateTime RatedAtUtc);
