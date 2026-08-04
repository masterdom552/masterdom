using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

namespace Masterdom.Modules.Tenancy.Application.Commands;

public sealed record RecordMoveOutCommand(
    TenancyId TenancyId,
    MoveOutDate MoveOutDate);
