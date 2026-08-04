namespace Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.ReadModels;

public sealed record RentChargeReadModel(
    Guid TenancyId,
    Guid LeaseId,
    Guid PropertyId,
    Guid UnitId,
    bool IsTenancyActive,
    bool IsLeaseActive,
    decimal? RentAmount,
    string? Currency,
    string? BillingFrequency,
    string? LeaseNumber);
