using Masterdom.Modules.Maintenance.Application.Queries;
using Masterdom.Modules.Maintenance.Application.Services;
using Masterdom.Modules.Maintenance.Application.Support;
using MaintenanceTicketAggregate = Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.MaintenanceTicket;

namespace Masterdom.Modules.Maintenance.Application.Handlers.Queries;

public sealed class GetMaintenanceTicketByIdQueryHandler : IQueryHandler<GetMaintenanceTicketByIdQuery, ExecutionResult<MaintenanceTicketAggregate>>
{
    private readonly IMaintenanceApplicationService _applicationService;

    public GetMaintenanceTicketByIdQueryHandler(IMaintenanceApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<MaintenanceTicketAggregate> Handle(GetMaintenanceTicketByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var maintenanceTicket = _applicationService.GetMaintenanceTicketById(query);
        return maintenanceTicket is null
            ? ExecutionResult<MaintenanceTicketAggregate>.Failure("not_found", $"Maintenance ticket '{query.MaintenanceTicketId}' was not found.")
            : ExecutionResult<MaintenanceTicketAggregate>.Success(maintenanceTicket);
    }
}
