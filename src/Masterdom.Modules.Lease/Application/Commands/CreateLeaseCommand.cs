using Masterdom.Modules.Lease.Domain.Entities.Lease;

namespace Masterdom.Modules.Lease.Application.Commands;

public sealed record CreateLeaseCommand(
    LeaseNumber Number,
    LeaseType Type,
    TenancyReference Tenancy,
    PropertyReference Property,
    UnitReference Unit,
    PersonReference Person,
    EffectivePeriod EffectivePeriod,
    CommercialTerms CommercialTerms,
    LeaseClauses LeaseClauses);
