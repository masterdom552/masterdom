using Masterdom.Core.Identifiers;
using Masterdom.Modules.Lease.Application.Commands;
using Masterdom.Modules.Lease.Application.Handlers.Commands;
using Masterdom.Modules.Lease.Application.Services;
using Masterdom.Modules.Lease.Application.Support;
using Masterdom.Modules.Lease.Domain.Entities.Lease;
using Masterdom.Modules.Lease.Domain.Repositories;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;

namespace Masterdom.Core.Tests.Lease;

public sealed class LeaseApplicationHandlersTests
{
    [Fact]
    public void CreateLeaseHandler_ShouldPersistNewLease()
    {
        var repository = new InMemoryLeaseRepository();
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();

        var service = new LeaseApplicationService(repository, unitOfWork, orchestrator);
        var handler = new CreateLeaseCommandHandler(service);

        var command = BuildCreateCommand("LS-APP-01", Guid.NewGuid());
        var result = handler.Handle(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, unitOfWork.ExecuteCount);
        Assert.Equal(1, orchestrator.MutationCount);
    }

    [Fact]
    public void RenewLeaseHandler_ShouldCreateNewVersion_WhenLeaseIsActive()
    {
        var repository = new InMemoryLeaseRepository();
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();
        var service = new LeaseApplicationService(repository, unitOfWork, orchestrator);

        var lease = service.CreateLease(BuildCreateCommand("LS-APP-02", Guid.NewGuid()));
        service.ActivateLease(new ActivateLeaseCommand(lease.Id));

        var handler = new RenewLeaseCommandHandler(service);
        var renew = new RenewLeaseCommand(
            lease.Id,
            RenewalDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(11))),
            EffectivePeriod.Create(
                EffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))),
                ExpiryDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(24)))),
            BuildCommercialTerms(1500m),
            BuildClauses("RENEW"));

        var result = handler.Handle(renew);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Versions.Count);
        Assert.Equal(3, unitOfWork.ExecuteCount);
    }

    private static CreateLeaseCommand BuildCreateCommand(string number, Guid tenancyId)
    {
        return new CreateLeaseCommand(
            LeaseNumber.Create(number),
            LeaseType.Residential,
            TenancyReference.Create(tenancyId),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            PersonReference.Create(PersonId.New()),
            EffectivePeriod.Create(
                EffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
                ExpiryDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)))),
            BuildCommercialTerms(1200m),
            BuildClauses("BASE"));
    }

    private static CommercialTerms BuildCommercialTerms(decimal monthlyRent)
    {
        return CommercialTerms.Create(
            RentTerms.Create(monthlyRent, BillingFrequency.Monthly, 5, 3),
            DepositTerms.Create(1000m, true, SecurityDepositReference.Create("DEP-001"), "config.deposit.default"),
            RenewalTerms.Create(false, 30, "config.renewal.standard"),
            TerminationTerms.Create(30, "config.termination.standard", "config.latefee.standard"));
    }

    private static LeaseClauses BuildClauses(string code)
    {
        return LeaseClauses.Create(
            ClauseCollection.Create([
                LeaseClause.Create(code, "Lease clause")
            ]));
    }

    private sealed class InMemoryLeaseRepository : ILeaseRepository
    {
        private readonly Dictionary<Guid, LeaseAggregate> _leases = [];

        public void Add(LeaseAggregate lease)
        {
            _leases[lease.Id.Value] = lease;
        }

        public LeaseAggregate? GetById(LeaseId id)
        {
            return _leases.TryGetValue(id.Value, out var lease) ? lease : null;
        }

        public LeaseAggregate? GetByNumber(LeaseNumber number)
        {
            return _leases.Values.FirstOrDefault(x => x.Number == number);
        }

        public bool HasActiveLeaseForTenancy(TenancyReference tenancy)
        {
            return _leases.Values.Any(x => x.Tenancy == tenancy && x.Status == LeaseStatus.Active);
        }

        public void Update(LeaseAggregate lease)
        {
            _leases[lease.Id.Value] = lease;
        }
    }

    private sealed class SpyUnitOfWork : ILeaseUnitOfWork
    {
        public int ExecuteCount { get; private set; }

        public void Execute(Action operation)
        {
            ExecuteCount++;
            operation();
        }
    }

    private sealed class SpyPlatformOrchestrator : ILeasePlatformOrchestrator
    {
        public int MutationCount { get; private set; }

        public void OnLeaseMutated(LeaseAggregate lease, string operationName)
        {
            MutationCount++;
        }
    }
}
