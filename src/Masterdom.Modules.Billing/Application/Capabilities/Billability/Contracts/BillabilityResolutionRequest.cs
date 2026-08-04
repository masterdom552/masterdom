using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;

public sealed record BillabilityResolutionRequest(
    BillingPeriod BillingPeriod,
    IReadOnlyCollection<BillabilityResolutionRequest.CandidateProjection> Candidates)
{
    public sealed record CandidateProjection(
        TenancyReference? TenancyReference,
        LeaseReference? LeaseReference,
        PropertyReference? PropertyReference,
        Guid? UnitId,
        PersonReference? PrimaryOccupantReference,
        string LeaseStatus,
        DateOnly LeaseEffectiveDate,
        DateOnly LeaseExpiryDate,
        string TenancyStatus,
        DateOnly MoveInDate,
        DateOnly? MoveOutDate,
        string UnitOccupancyStatus);
}
