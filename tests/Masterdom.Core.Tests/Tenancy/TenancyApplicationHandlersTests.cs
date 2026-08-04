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
