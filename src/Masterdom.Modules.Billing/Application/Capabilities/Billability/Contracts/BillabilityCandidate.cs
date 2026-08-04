using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;

public sealed record BillabilityCandidate(
    TenancyReference? TenancyReference,
    LeaseReference? LeaseReference,
    PropertyReference? PropertyReference,
    Guid? UnitId,
    PersonReference? PrimaryOccupantReference);
