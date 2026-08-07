using Masterdom.Modules.Maintenance.Application.Commands;
using Masterdom.Modules.Maintenance.Application.Queries;
using Masterdom.Modules.Maintenance.Application.Support;
using Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;
using Masterdom.Modules.Maintenance.Domain.Repositories;
using MaintenanceTicketAggregate = Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.MaintenanceTicket;

namespace Masterdom.Modules.Maintenance.Application.Services;

public sealed class MaintenanceApplicationService : IMaintenanceApplicationService
{
    private readonly IMaintenanceTicketRepository _repository;
    private readonly IMaintenanceUnitOfWork _unitOfWork;
    private readonly IMaintenancePlatformOrchestrator _platformOrchestrator;

    public MaintenanceApplicationService(
        IMaintenanceTicketRepository repository,
        IMaintenanceUnitOfWork unitOfWork,
        IMaintenancePlatformOrchestrator platformOrchestrator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public MaintenanceTicketAggregate CreateMaintenanceTicket(CreateMaintenanceTicketCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var maintenanceTicket = MaintenanceTicketAggregate.Create(
            MaintenanceTicketId.New(),
            command.PropertyId,
            command.UnitId,
            command.Title,
            command.Description,
            command.CreatedAtUtc);

        _unitOfWork.Execute(() =>
        {
            _repository.Add(maintenanceTicket);
        });

        _platformOrchestrator.OnMaintenanceTicketMutated(maintenanceTicket, "CreateMaintenanceTicket");
        return maintenanceTicket;
    }

    public MaintenanceTicketAggregate AssignMaintenanceTicket(AssignMaintenanceTicketCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var maintenanceTicket = _repository.GetById(command.MaintenanceTicketId);
        if (maintenanceTicket is null)
        {
            throw new InvalidOperationException($"Maintenance ticket '{command.MaintenanceTicketId}' was not found.");
        }

        maintenanceTicket.Assign(command.AssignedToPersonId, command.AssignedAtUtc);

        _unitOfWork.Execute(() =>
        {
            _repository.Update(maintenanceTicket);
        });

        _platformOrchestrator.OnMaintenanceTicketMutated(maintenanceTicket, "AssignMaintenanceTicket");
        return maintenanceTicket;
    }

    public MaintenanceTicketAggregate CloseMaintenanceTicket(CloseMaintenanceTicketCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var maintenanceTicket = _repository.GetById(command.MaintenanceTicketId);
        if (maintenanceTicket is null)
        {
            throw new InvalidOperationException($"Maintenance ticket '{command.MaintenanceTicketId}' was not found.");
        }

        maintenanceTicket.Close(command.ClosedAtUtc);

        _unitOfWork.Execute(() =>
        {
            _repository.Update(maintenanceTicket);
        });

        _platformOrchestrator.OnMaintenanceTicketMutated(maintenanceTicket, "CloseMaintenanceTicket");
        return maintenanceTicket;
    }

    public MaintenanceTicketAggregate? GetMaintenanceTicketById(GetMaintenanceTicketByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetById(query.MaintenanceTicketId);
    }
}
