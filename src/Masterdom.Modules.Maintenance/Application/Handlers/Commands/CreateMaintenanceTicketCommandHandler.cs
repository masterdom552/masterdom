using Masterdom.Modules.Maintenance.Application.Commands;
using Masterdom.Modules.Maintenance.Application.Services;
using Masterdom.Modules.Maintenance.Application.Support;
using MaintenanceTicketAggregate = Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.MaintenanceTicket;

namespace Masterdom.Modules.Maintenance.Application.Handlers.Commands;

public sealed class CreateMaintenanceTicketCommandHandler : ICommandHandler<CreateMaintenanceTicketCommand, ExecutionResult<MaintenanceTicketAggregate>>
{
    private readonly IMaintenanceApplicationService _applicationService;

    public CreateMaintenanceTicketCommandHandler(IMaintenanceApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<MaintenanceTicketAggregate> Handle(CreateMaintenanceTicketCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var maintenanceTicket = _applicationService.CreateMaintenanceTicket(command);
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
