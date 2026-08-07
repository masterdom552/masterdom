using Masterdom.Modules.Maintenance.Application.Commands;
using Masterdom.Modules.Maintenance.Application.Handlers.Commands;
using Masterdom.Modules.Maintenance.Application.Handlers.Queries;
using Masterdom.Modules.Maintenance.Application.Queries;
using Masterdom.Modules.Maintenance.Application.Services;
using Masterdom.Modules.Maintenance.Application.Support;
using Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;
using Masterdom.Modules.Maintenance.Domain.Repositories;
using MaintenanceTicketAggregate = Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.MaintenanceTicket;

namespace Masterdom.Core.Tests.Maintenance;

public sealed class MaintenanceApplicationHandlersTests
{
    [Fact]
    public void CreateAndGetById_ShouldPersistAndReturnMaintenanceTicket()
    {
        var repository = new InMemoryMaintenanceTicketRepository();
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();

        var service = new MaintenanceApplicationService(repository, unitOfWork, orchestrator);
        var createHandler = new CreateMaintenanceTicketCommandHandler(service);
        var assignHandler = new AssignMaintenanceTicketCommandHandler(service);
        var closeHandler = new CloseMaintenanceTicketCommandHandler(service);
        var getByIdHandler = new GetMaintenanceTicketByIdQueryHandler(service);

        var createResult = createHandler.Handle(new CreateMaintenanceTicketCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "HVAC issue",
            "HVAC not cooling in unit 204.",
            DateTime.UtcNow));

        Assert.True(createResult.IsSuccess);
        Assert.NotNull(createResult.Value);
        Assert.Equal(1, unitOfWork.ExecuteCount);
        Assert.Equal(1, orchestrator.MutationCount);

        var assignedToPersonId = Guid.NewGuid();
        var assignedAtUtc = DateTime.UtcNow;
        var assignResult = assignHandler.Handle(
            new AssignMaintenanceTicketCommand(
                createResult.Value.Id,
                assignedToPersonId,
                assignedAtUtc));

        Assert.True(assignResult.IsSuccess);
        Assert.NotNull(assignResult.Value);
        Assert.Equal(2, unitOfWork.ExecuteCount);
        Assert.Equal(2, orchestrator.MutationCount);
        Assert.Equal(assignedToPersonId, assignResult.Value!.AssignedToPersonId);
        Assert.Equal(assignedAtUtc, assignResult.Value.AssignedAtUtc);

        var closeResult = closeHandler.Handle(
            new CloseMaintenanceTicketCommand(
                createResult.Value.Id,
                DateTime.UtcNow));

        Assert.True(closeResult.IsSuccess);
        Assert.NotNull(closeResult.Value);
        Assert.Equal(3, unitOfWork.ExecuteCount);
        Assert.Equal(3, orchestrator.MutationCount);
        Assert.Equal(MaintenanceTicketStatus.Closed, closeResult.Value!.Status);

        var getResult = getByIdHandler.Handle(
            new GetMaintenanceTicketByIdQuery(createResult.Value!.Id));

        Assert.True(getResult.IsSuccess);
        Assert.NotNull(getResult.Value);
        Assert.Equal(createResult.Value.Id, getResult.Value!.Id);
    }

    private sealed class InMemoryMaintenanceTicketRepository : IMaintenanceTicketRepository
    {
        private readonly Dictionary<Guid, MaintenanceTicketAggregate> _maintenanceTickets = [];

        public void Add(MaintenanceTicketAggregate maintenanceTicket)
        {
            _maintenanceTickets[maintenanceTicket.Id.Value] = maintenanceTicket;
        }

        public void Update(MaintenanceTicketAggregate maintenanceTicket)
        {
            _maintenanceTickets[maintenanceTicket.Id.Value] = maintenanceTicket;
        }

        public MaintenanceTicketAggregate? GetById(MaintenanceTicketId id)
        {
            return _maintenanceTickets.TryGetValue(id.Value, out var maintenanceTicket)
                ? maintenanceTicket
                : null;
        }
    }

    private sealed class SpyUnitOfWork : IMaintenanceUnitOfWork
    {
        public int ExecuteCount { get; private set; }

        public void Execute(Action operation)
        {
            ExecuteCount++;
            operation();
        }
    }

    private sealed class SpyPlatformOrchestrator : IMaintenancePlatformOrchestrator
    {
        public int MutationCount { get; private set; }

        public void OnMaintenanceTicketMutated(MaintenanceTicketAggregate maintenanceTicket, string operationName)
        {
            MutationCount++;
        }
    }
}
