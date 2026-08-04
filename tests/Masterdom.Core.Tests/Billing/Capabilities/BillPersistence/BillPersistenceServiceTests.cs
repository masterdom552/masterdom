using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence;
using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence.Contracts;
using Masterdom.Modules.Billing.Application.Publication;
using Masterdom.Modules.Billing.Application.Events;
using Masterdom.Modules.Billing.Contracts.Published.Notifications;
using Masterdom.Modules.Billing.Application.Support;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using Masterdom.Modules.Billing.Domain.Repositories;
using Masterdom.Core.Identifiers;
using Masterdom.Core.Financial.ValueObjects;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Core.Tests.Billing.MonthlyBilling.Capabilities.BillPersistence;

public sealed class MonthlyBillingBillPersistenceServiceTests
{
    [Fact]
    public void Persist_ShouldDelegateExecutionToBillPersistenceOperation()
    {
        var operation = new SpyBillPersistenceOperation();
        var projector = new SpyBillingNotificationProjector();
        var platform = new SpyBillingPlatformOrchestrator();
        var service = new BillPersistenceService(operation, platform, projector);
        var request = new BillPersistenceRequest(Array.Empty<BillAggregate>());

        service.Persist(request);

        Assert.Equal(1, operation.ExecuteCallCount);
        Assert.Same(request, operation.LastRequest);
    }

    [Fact]
    public void Persist_ShouldReturnCallerFocusedResultFromPersistedBills()
    {
        var persistedBills = new[] { CreateBill("BILL-PERSIST-SVC-001"), CreateBill("BILL-PERSIST-SVC-002") };
        var operation = new SpyBillPersistenceOperation(persistedBills);
        var projector = new SpyBillingNotificationProjector();
        var platform = new SpyBillingPlatformOrchestrator();
        var service = new BillPersistenceService(operation, platform, projector);

        var result = service.Persist(new BillPersistenceRequest(Array.Empty<BillAggregate>()));

        Assert.Equal(2, result.PersistedCount);
        Assert.Equal(2, result.PersistedBills.Count);
    }

    [Fact]
    public void Persist_ShouldPublishInternalApplicationEvent_AfterOperationCompletes()
    {
        var persistedBill = CreateBill("BILL-PERSIST-SVC-003");
        var operation = new SpyBillPersistenceOperation([persistedBill]);
        var projector = new SpyBillingNotificationProjector();
        var platform = new SpyBillingPlatformOrchestrator();
        var service = new BillPersistenceService(operation, platform, projector);

        service.Persist(new BillPersistenceRequest([persistedBill]));

        var applicationEvent = Assert.Single(platform.PublishedEvents);
        Assert.Equal("MonthlyBillingPersistBills", applicationEvent.CorrelationId);
        Assert.Equal(persistedBill.CurrentSnapshot.BillingPeriod, applicationEvent.BillingPeriod);
        Assert.Contains(persistedBill.Id, applicationEvent.PersistedBillIds);
        Assert.Equal(1, projector.ProjectCallCount);
    }

    [Fact]
    public void Persist_ShouldNotPublishOrProject_WhenNoBillsPersisted()
    {
        var operation = new SpyBillPersistenceOperation(Array.Empty<BillAggregate>());
        var projector = new SpyBillingNotificationProjector();
        var platform = new SpyBillingPlatformOrchestrator();
        var service = new BillPersistenceService(operation, platform, projector);

        var result = service.Persist(new BillPersistenceRequest(Array.Empty<BillAggregate>()));

        Assert.Equal(0, result.PersistedCount);
        Assert.Empty(platform.PublishedEvents);
        Assert.Equal(0, projector.ProjectCallCount);
    }

    private sealed class SpyBillPersistenceOperation : BillPersistenceOperation
    {
        private readonly IReadOnlyCollection<BillAggregate> _result;

        public SpyBillPersistenceOperation(IReadOnlyCollection<BillAggregate>? result = null)
            : base(new NullBillRepository(), new NullBillingUnitOfWork())
        {
            _result = result ?? Array.Empty<BillAggregate>();
        }

        public int ExecuteCallCount { get; private set; }

        public BillPersistenceRequest? LastRequest { get; private set; }

        public override IReadOnlyCollection<BillAggregate> Execute(BillPersistenceRequest request)
        {
            ExecuteCallCount++;
            LastRequest = request;
            return _result;
        }
    }

    private sealed class SpyBillingNotificationProjector : BillingNotificationProjector
    {
        public int ProjectCallCount { get; private set; }

        public override BillPersistedNotification ProjectBillPersisted(
            string correlationId,
            IReadOnlyCollection<BillAggregate> persistedBills,
            DateTime executionTimestampUtc)
        {
            ProjectCallCount++;

            return new BillPersistedNotification(
                correlationId,
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 8, 31),
                persistedBills.Select(x => x.Id.Value).ToList(),
                persistedBills.Count,
                executionTimestampUtc);
        }
    }

    private sealed class SpyBillingPlatformOrchestrator : IBillingPlatformOrchestrator
    {
        public List<IBillingApplicationEvent> PublishedEvents { get; } = [];

        public void OnBillMutated(BillAggregate bill, string operationName)
        {
        }

        public void Publish(IBillingApplicationEvent applicationEvent)
        {
            PublishedEvents.Add(applicationEvent);
        }
    }

    private sealed class NullBillRepository : IBillRepository
    {
        public void Add(BillAggregate bill)
        {
        }

        public BillAggregate? GetById(BillId id)
        {
            return null;
        }

        public BillAggregate? GetByNumber(BillNumber number)
        {
            return null;
        }

        public void Update(BillAggregate bill)
        {
        }
    }

    private sealed class NullBillingUnitOfWork : IBillingUnitOfWork
    {
        public void Execute(Action operation)
        {
            operation();
        }
    }

    private static BillAggregate CreateBill(string billNumber)
    {
        return BillAggregate.Generate(
            BillId.New(),
            BillNumber.Create(billNumber),
            TenancyReference.Create(Guid.NewGuid()),
            LeaseReference.Create(Guid.NewGuid()),
            PropertyReference.Create(Guid.NewGuid()),
            PersonReference.Create(PersonId.New()),
            BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),
            BillingCycle.Monthly,
            GeneratedDate.Create(new DateOnly(2026, 8, 1)),
            IssueDate.Create(new DateOnly(2026, 8, 1)),
            DueDate.Create(new DateOnly(2026, 8, 10)),
            Currency.Create("USD"),
            ChargeCollection.Create([ChargeLine.Create(ChargeKind.Rent, "Rent charge", 1000m)]));
    }
}
