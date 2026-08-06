using Masterdom.Core.Identifiers;
using Masterdom.Modules.Tenancy.Application.Commands;
using Masterdom.Modules.Tenancy.Application.Handlers.Commands;
using Masterdom.Modules.Tenancy.Application.Services;
using Masterdom.Modules.Tenancy.Application.Support;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using Masterdom.Modules.Tenancy.Domain.Repositories;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Core.Tests.Tenancy;

public sealed class TenancyApplicationHandlersTests
{
    [Fact]
    public void CreateTenancyHandler_ShouldPersistTenancy()
    {
        var repository = new InMemoryTenancyRepository();
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();

        var service = new TenancyApplicationService(repository, unitOfWork, orchestrator);
        var handler = new CreateTenancyCommandHandler(service);

        var command = new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-01"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            PersonId.New(),
            Notes.Create("Tenancy notes"));

        var result = handler.Handle(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, unitOfWork.ExecuteCount);
        Assert.Equal(1, orchestrator.MutationCount);
    }

    [Fact]
    public void RecordMoveOutHandler_ShouldUpdateOccupancy()
    {
        var repository = new InMemoryTenancyRepository();
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();
        var service = new TenancyApplicationService(repository, unitOfWork, orchestrator);

        var create = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-02"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5))),
            PersonId.New(),
            Notes.Create(null)));

        var handler = new RecordMoveOutCommandHandler(service);
        var result = handler.Handle(new RecordMoveOutCommand(
            create.Id,
            MoveOutDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(OccupancyStatus.Vacated, result.Value.OccupancyStatus);
    }

    [Fact]
    public void UpdateNotesHandler_ShouldUpdateNotes()
    {
        var repository = new InMemoryTenancyRepository();
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();
        var service = new TenancyApplicationService(repository, unitOfWork, orchestrator);

        var tenancy = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-03"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            PersonId.New(),
            Notes.Create(null)));

        var handler = new UpdateTenancyNotesCommandHandler(service);
        var result = handler.Handle(new UpdateTenancyNotesCommand(tenancy.Id, Notes.Create("Updated")));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Updated", result.Value.Notes?.Value);
        Assert.Equal(2, unitOfWork.ExecuteCount);
    }

    [Fact]
    public void UpdateNotesHandler_ShouldClearNotes_WhenPassedNull()
    {
        var repository = new InMemoryTenancyRepository();
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();
        var service = new TenancyApplicationService(repository, unitOfWork, orchestrator);

        var tenancy = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-04"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            PersonId.New(),
            Notes.Create("Initial notes")));

        var handler = new UpdateTenancyNotesCommandHandler(service);
        var result = handler.Handle(new UpdateTenancyNotesCommand(tenancy.Id, null));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value.Notes);
    }

    [Fact]
    public void UpdateNotesHandler_ShouldReturnConflict_WhenTenancyIsArchived()
    {
        var repository = new InMemoryTenancyRepository();
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();
        var service = new TenancyApplicationService(repository, unitOfWork, orchestrator);

        var tenancy = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-05"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            PersonId.New(),
            Notes.Create(null)));

        service.CloseTenancy(new CloseTenancyCommand(
            tenancy.Id,
            EffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))),
            TerminationReason.Create("Test")));
        service.ArchiveTenancy(new ArchiveTenancyCommand(tenancy.Id));

        var handler = new UpdateTenancyNotesCommandHandler(service);
        var result = handler.Handle(new UpdateTenancyNotesCommand(tenancy.Id, Notes.Create("Cannot update")));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
    }

    [Fact]
    public void CreateTenancyHandler_ShouldReturnConflict_WhenUnitAlreadyHasActiveTenancy()
    {
        var repository = new InMemoryTenancyRepository();
        var service = new TenancyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator());
        var handler = new CreateTenancyCommandHandler(service);

        var unitRef = UnitReference.Create(Guid.NewGuid());
        var propertyRef = PropertyReference.Create(Guid.NewGuid());

        handler.Handle(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-06"),
            propertyRef,
            unitRef,
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            PersonId.New(),
            Notes.Create(null)));

        var duplicate = handler.Handle(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-07"),
            propertyRef,
            unitRef,
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            PersonId.New(),
            Notes.Create(null)));

        Assert.False(duplicate.IsSuccess);
        Assert.Equal("conflict", duplicate.ErrorCode);
    }

    [Fact]
    public void RecordMoveOutHandler_ShouldReturnConflict_WhenMoveOutIsBeforeMoveIn()
    {
        var repository = new InMemoryTenancyRepository();
        var service = new TenancyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator());

        var tenancy = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-08"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            PersonId.New(),
            Notes.Create(null)));

        var handler = new RecordMoveOutCommandHandler(service);
        var result = handler.Handle(new RecordMoveOutCommand(
            tenancy.Id,
            MoveOutDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)))));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
    }

    [Fact]
    public void AddOccupantHandler_ShouldAddOccupantToTenancy()
    {
        var repository = new InMemoryTenancyRepository();
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();
        var service = new TenancyApplicationService(repository, unitOfWork, orchestrator);

        var tenancy = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-09"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            PersonId.New(),
            Notes.Create(null)));

        var newOccupant = PersonId.New();
        var handler = new AddOccupantCommandHandler(service);
        var result = handler.Handle(new AddOccupantCommand(tenancy.Id, newOccupant, false));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Occupants.Count);
        Assert.Contains(result.Value.Occupants, o => o.PersonId == newOccupant);
        Assert.Equal(2, unitOfWork.ExecuteCount);
    }

    [Fact]
    public void AddOccupantHandler_ShouldReturnConflict_WhenOccupantAlreadyExists()
    {
        var repository = new InMemoryTenancyRepository();
        var service = new TenancyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator());

        var primaryId = PersonId.New();
        var tenancy = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-10"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            primaryId,
            Notes.Create(null)));

        var handler = new AddOccupantCommandHandler(service);
        var result = handler.Handle(new AddOccupantCommand(tenancy.Id, primaryId, false));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
    }

    [Fact]
    public void RemoveOccupantHandler_ShouldRemoveSecondaryOccupant()
    {
        var repository = new InMemoryTenancyRepository();
        var unitOfWork = new SpyUnitOfWork();
        var service = new TenancyApplicationService(repository, unitOfWork, new SpyPlatformOrchestrator());

        var primaryId = PersonId.New();
        var secondaryId = PersonId.New();

        var tenancy = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-11"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            primaryId,
            Notes.Create(null)));

        service.AddOccupant(new AddOccupantCommand(tenancy.Id, secondaryId, false));

        var handler = new RemoveOccupantCommandHandler(service);
        var result = handler.Handle(new RemoveOccupantCommand(tenancy.Id, secondaryId));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Single(tenancy.Occupants);
    }

    [Fact]
    public void RemoveOccupantHandler_ShouldReturnConflict_WhenRemovingOnlyPrimaryOccupant()
    {
        var repository = new InMemoryTenancyRepository();
        var service = new TenancyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator());

        var primaryId = PersonId.New();
        var tenancy = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-12"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            primaryId,
            Notes.Create(null)));

        var handler = new RemoveOccupantCommandHandler(service);
        var result = handler.Handle(new RemoveOccupantCommand(tenancy.Id, primaryId));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
    }

    [Fact]
    public void RecordMoveInHandler_ShouldUpdateMoveInDateAndSetOccupied()
    {
        var repository = new InMemoryTenancyRepository();
        var unitOfWork = new SpyUnitOfWork();
        var service = new TenancyApplicationService(repository, unitOfWork, new SpyPlatformOrchestrator());

        var tenancy = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-13"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))),
            PersonId.New(),
            Notes.Create(null)));

        var actualMoveIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var handler = new RecordMoveInCommandHandler(service);
        var result = handler.Handle(new RecordMoveInCommand(tenancy.Id, MoveInDate.Create(actualMoveIn)));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(actualMoveIn, result.Value.MoveInDate.Value);
        Assert.Equal(OccupancyStatus.Occupied, result.Value.OccupancyStatus);
        Assert.Equal(2, unitOfWork.ExecuteCount);
    }

    [Fact]
    public void RecordMoveInHandler_ShouldReturnConflict_WhenTenancyIsClosed()
    {
        var repository = new InMemoryTenancyRepository();
        var service = new TenancyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator());

        var tenancy = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-14"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            PersonId.New(),
            Notes.Create(null)));

        service.CloseTenancy(new CloseTenancyCommand(
            tenancy.Id,
            EffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))),
            TerminationReason.Create("Closed")));

        var handler = new RecordMoveInCommandHandler(service);
        var result = handler.Handle(new RecordMoveInCommand(
            tenancy.Id,
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)))));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
    }

    [Fact]
    public void CloseTenancyHandler_ShouldCloseWithReasonAndDate()
    {
        var repository = new InMemoryTenancyRepository();
        var unitOfWork = new SpyUnitOfWork();
        var service = new TenancyApplicationService(repository, unitOfWork, new SpyPlatformOrchestrator());

        var tenancy = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-15"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            PersonId.New(),
            Notes.Create(null)));

        var closedOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var handler = new CloseTenancyCommandHandler(service);
        var result = handler.Handle(new CloseTenancyCommand(
            tenancy.Id,
            EffectiveDate.Create(closedOn),
            TerminationReason.Create("Lease ended")));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(TenancyStatus.Closed, result.Value.Status);
        Assert.Equal(closedOn, result.Value.ClosedOn?.Value);
        Assert.Equal("Lease ended", result.Value.TerminationReason?.Value);
        Assert.Equal(2, unitOfWork.ExecuteCount);
    }

    [Fact]
    public void CloseTenancyHandler_ShouldReturnConflict_WhenClosedOnIsBeforeMoveIn()
    {
        var repository = new InMemoryTenancyRepository();
        var service = new TenancyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator());

        var tenancy = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-16"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            PersonId.New(),
            Notes.Create(null)));

        var handler = new CloseTenancyCommandHandler(service);
        var result = handler.Handle(new CloseTenancyCommand(
            tenancy.Id,
            EffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))),
            TerminationReason.Create("Invalid")));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
    }

    [Fact]
    public void ArchiveTenancyHandler_ShouldArchiveClosedTenancy()
    {
        var repository = new InMemoryTenancyRepository();
        var unitOfWork = new SpyUnitOfWork();
        var service = new TenancyApplicationService(repository, unitOfWork, new SpyPlatformOrchestrator());

        var tenancy = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-17"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            PersonId.New(),
            Notes.Create(null)));

        service.CloseTenancy(new CloseTenancyCommand(
            tenancy.Id,
            EffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))),
            TerminationReason.Create("Complete")));

        var handler = new ArchiveTenancyCommandHandler(service);
        var result = handler.Handle(new ArchiveTenancyCommand(tenancy.Id));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(TenancyStatus.Archived, result.Value.Status);
        Assert.Equal(3, unitOfWork.ExecuteCount);
    }

    [Fact]
    public void ArchiveTenancyHandler_ShouldReturnConflict_WhenTenancyIsActive()
    {
        var repository = new InMemoryTenancyRepository();
        var service = new TenancyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator());

        var tenancy = service.CreateTenancy(new CreateTenancyCommand(
            TenancyNumber.Create("TEN-APP-18"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            PersonId.New(),
            Notes.Create(null)));

        var handler = new ArchiveTenancyCommandHandler(service);
        var result = handler.Handle(new ArchiveTenancyCommand(tenancy.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
    }

    private sealed class InMemoryTenancyRepository : ITenancyRepository
    {
        private readonly Dictionary<Guid, TenancyAggregate> _tenancies = [];

        public void Add(TenancyAggregate tenancy)
        {
            _tenancies[tenancy.Id.Value] = tenancy;
        }

        public TenancyAggregate? GetById(TenancyId id)
        {
            return _tenancies.TryGetValue(id.Value, out var tenancy) ? tenancy : null;
        }

        public bool HasActiveTenancyForUnit(UnitReference unit)
        {
            return _tenancies.Values.Any(x => x.Unit == unit && x.Status == TenancyStatus.Active);
        }

        public void Update(TenancyAggregate tenancy)
        {
            _tenancies[tenancy.Id.Value] = tenancy;
        }
    }

    private sealed class SpyUnitOfWork : ITenancyUnitOfWork
    {
        public int ExecuteCount { get; private set; }

        public void Execute(Action operation)
        {
            ExecuteCount++;
            operation();
        }
    }

    private sealed class SpyPlatformOrchestrator : ITenancyPlatformOrchestrator
    {
        public int MutationCount { get; private set; }

        public void OnTenancyMutated(TenancyAggregate tenancy, string operationName)
        {
            MutationCount++;
        }
    }
}
