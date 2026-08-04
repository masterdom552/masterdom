using Masterdom.Core.Identifiers;
using Masterdom.Core.Financial.ValueObjects;
using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence;
using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence.Contracts;
using Masterdom.Modules.Billing.Application.Support;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using Masterdom.Modules.Billing.Domain.Repositories;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Core.Tests.Billing.MonthlyBilling.Capabilities.BillPersistence;

public sealed class MonthlyBillingBillPersistenceOperationTests
{
    [Fact]
    public void Execute_ShouldPersistBills_AndCallRepositoryOncePerBill()
    {
        var executionOrder = new List<string>();
        var repository = new TrackingBillRepository(executionOrder);
        var unitOfWork = new TrackingBillingUnitOfWork(executionOrder);
        var operation = new BillPersistenceOperation(repository, unitOfWork);

        var bills = new[] { CreateBill("BILL-PERSIST-OP-001"), CreateBill("BILL-PERSIST-OP-002") };

        var result = operation.Execute(new BillPersistenceRequest(bills));

        Assert.Equal(2, result.Count);
        Assert.Equal(2, repository.AddedBills.Count);
        Assert.Equal(1, unitOfWork.ExecuteCount);
    }

    [Fact]
    public void Execute_ShouldCommit_AfterPersistence()
    {
        var executionOrder = new List<string>();
        var repository = new TrackingBillRepository(executionOrder);
        var unitOfWork = new TrackingBillingUnitOfWork(executionOrder);
        var operation = new BillPersistenceOperation(repository, unitOfWork);

        operation.Execute(new BillPersistenceRequest([CreateBill("BILL-PERSIST-OP-003")]));

        Assert.Equal(
            new[]
            {
                "UnitOfWork.Begin",
                "Repository.Add",
                "UnitOfWork.Commit"
            },
            executionOrder);
    }

    [Fact]
    public void Execute_ShouldPropagate_WhenPersistenceFails()
    {
        var executionOrder = new List<string>();
        var repository = new TrackingBillRepository(executionOrder) { ThrowOnAdd = true };
        var unitOfWork = new TrackingBillingUnitOfWork(executionOrder);
        var operation = new BillPersistenceOperation(repository, unitOfWork);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            operation.Execute(new BillPersistenceRequest([CreateBill("BILL-PERSIST-OP-004")])));

        Assert.Equal("Simulated persistence failure.", exception.Message);
    }

    [Fact]
    public void Execute_ShouldPropagate_WhenCommitFails()
    {
        var executionOrder = new List<string>();
        var repository = new TrackingBillRepository(executionOrder);
        var unitOfWork = new TrackingBillingUnitOfWork(executionOrder) { ThrowOnCommit = true };
        var operation = new BillPersistenceOperation(repository, unitOfWork);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            operation.Execute(new BillPersistenceRequest([CreateBill("BILL-PERSIST-OP-005")])));

        Assert.Equal("Simulated commit failure.", exception.Message);
    }

    [Fact]
    public void Execute_ShouldDoNothing_WhenInputIsEmpty()
    {
        var executionOrder = new List<string>();
        var repository = new TrackingBillRepository(executionOrder);
        var unitOfWork = new TrackingBillingUnitOfWork(executionOrder);
        var operation = new BillPersistenceOperation(repository, unitOfWork);

        var result = operation.Execute(new BillPersistenceRequest(Array.Empty<BillAggregate>()));

        Assert.Empty(result);
        Assert.Equal(0, unitOfWork.ExecuteCount);
        Assert.Empty(repository.AddedBills);
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

    private sealed class TrackingBillRepository : IBillRepository
    {
        private readonly List<string> _executionOrder;

        public TrackingBillRepository(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public List<BillAggregate> AddedBills { get; } = [];

        public bool ThrowOnAdd { get; set; }

        public void Add(BillAggregate bill)
        {
            _executionOrder.Add("Repository.Add");

            if (ThrowOnAdd)
            {
                throw new InvalidOperationException("Simulated persistence failure.");
            }

            AddedBills.Add(bill);
        }

        public BillAggregate? GetById(BillId id)
        {
            return AddedBills.FirstOrDefault(x => x.Id == id);
        }

        public BillAggregate? GetByNumber(BillNumber number)
        {
            return AddedBills.FirstOrDefault(x => x.BillNumber == number);
        }

        public void Update(BillAggregate bill)
        {
            throw new NotSupportedException("Update is not required for bill persistence operation tests.");
        }
    }

    private sealed class TrackingBillingUnitOfWork : IBillingUnitOfWork
    {
        private readonly List<string> _executionOrder;

        public TrackingBillingUnitOfWork(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public int ExecuteCount { get; private set; }

        public bool CommitCompleted { get; private set; }

        public bool ThrowOnCommit { get; set; }

        public void Execute(Action operation)
        {
            ArgumentNullException.ThrowIfNull(operation);

            ExecuteCount++;
            CommitCompleted = false;
            _executionOrder.Add("UnitOfWork.Begin");

            operation();

            if (ThrowOnCommit)
            {
                throw new InvalidOperationException("Simulated commit failure.");
            }

            CommitCompleted = true;
            _executionOrder.Add("UnitOfWork.Commit");
        }
    }

}
