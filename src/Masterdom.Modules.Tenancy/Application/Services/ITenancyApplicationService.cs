using Masterdom.Modules.Tenancy.Application.Commands;
using Masterdom.Modules.Tenancy.Application.Queries;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Modules.Tenancy.Application.Services;

/// <summary>
/// Defines application orchestration boundary for tenancy use-cases.
/// </summary>
public interface ITenancyApplicationService
{
    TenancyAggregate CreateTenancy(CreateTenancyCommand command);

    TenancyAggregate AddOccupant(AddOccupantCommand command);

    bool RemoveOccupant(RemoveOccupantCommand command);

    TenancyAggregate RecordMoveIn(RecordMoveInCommand command);

    TenancyAggregate RecordMoveOut(RecordMoveOutCommand command);

    TenancyAggregate CloseTenancy(CloseTenancyCommand command);

    TenancyAggregate ArchiveTenancy(ArchiveTenancyCommand command);

    TenancyAggregate? GetTenancy(GetTenancyByIdQuery query);
}
