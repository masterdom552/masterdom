using Masterdom.Modules.Lease.Domain.Entities.Lease;

namespace Masterdom.Modules.Lease.Application.Commands;

public sealed record ChangeCommercialTermsCommand(
    LeaseId LeaseId,
    CommercialTerms CommercialTerms,
    EffectivePeriod EffectivePeriod);
