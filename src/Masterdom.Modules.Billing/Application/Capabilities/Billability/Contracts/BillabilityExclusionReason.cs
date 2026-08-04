namespace Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;

public enum BillabilityExclusionReason
{
    InactiveLease = 1,
    InactiveTenancy = 2,
    FutureLease = 3,
    ExpiredLease = 4,
    VacantUnit = 5,
    NoPrimaryOccupant = 6,
    OutsideBillingPeriod = 7,
    MissingReference = 8
}
