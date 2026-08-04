using Masterdom.Modules.Lease.Domain.Entities.Lease;

namespace Masterdom.Modules.Lease.Application.Commands;

public sealed record RenewLeaseCommand(
    LeaseId LeaseId,
    RenewalDate RenewalDate,
    EffectivePeriod EffectivePeriod,
    CommercialTerms CommercialTerms,
    LeaseClauses LeaseClauses);
