using Masterdom.Modules.Maintenance.Application.Commands;
using Masterdom.Modules.Maintenance.Application.Services;
using Masterdom.Modules.Maintenance.Application.Support;
using MaintenanceTicketAggregate = Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.MaintenanceTicket;

namespace Masterdom.Modules.Maintenance.Application.Handlers.Commands;

public sealed class AssignMaintenanceTicketCommandHandler : ICommandHandler<AssignMaintenanceTicketCommand, ExecutionResult<MaintenanceTicketAggregate>>
{
    private readonly IMaintenanceApplicationService _applicationService;

    public AssignMaintenanceTicketCommandHandler(IMaintenanceApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<MaintenanceTicketAggregate> Handle(AssignMaintenanceTicketCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var maintenanceTicket = _applicationService.AssignMaintenanceTicket(command);
            return ExecutionResult<MaintenanceTicketAggregate>.Success(maintenanceTicket);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<MaintenanceTicketAggregate>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<MaintenanceTicketAggregate>.Failure("conflict", ex.Message);
        }
    }
}
