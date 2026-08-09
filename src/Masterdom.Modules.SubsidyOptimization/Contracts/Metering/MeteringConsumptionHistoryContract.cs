namespace Masterdom.Modules.SubsidyOptimization.Contracts.Metering;

public sealed record MeteringConsumptionHistoryContract(
    Guid MeterId,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal TotalConsumptionUnits,
    DateTime CapturedAtUtc,
    string MeterType,
    string MeterStatus,
    decimal? SanctionedLoad);
