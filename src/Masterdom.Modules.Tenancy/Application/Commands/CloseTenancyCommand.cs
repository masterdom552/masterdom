using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

namespace Masterdom.Modules.Tenancy.Application.Commands;

public sealed record CloseTenancyCommand(
    TenancyId TenancyId,
    EffectiveDate ClosedOn,
    TerminationReason Reason);
