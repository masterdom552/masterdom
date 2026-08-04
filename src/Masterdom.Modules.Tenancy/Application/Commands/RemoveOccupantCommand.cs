using Masterdom.Core.Identifiers;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

namespace Masterdom.Modules.Tenancy.Application.Commands;

public sealed record RemoveOccupantCommand(
    TenancyId TenancyId,
    PersonId PersonId);
