using Masterdom.Core.Identifiers;
using Masterdom.Core.Financial.ValueObjects;
using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence;
using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence.Contracts;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Core.Tests.Billing.MonthlyBilling.Capabilities.BillPersistence;

public sealed class MonthlyBillingBillPersistenceCapabilityTests
{
    [Fact]
    public void Persist_ShouldDelegateToBillPersistenceService()
    {
        var service = new SpyBillPersistenceService();
        var capability = new BillPersistenceCapability(service);

        var bills = new[] { CreateBill("BILL-PERSIST-001"), CreateBill("BILL-PERSIST-002") };

        var result = capability.Persist(new BillPersistenceRequest(bills));

        Assert.Equal(1, service.PersistCallCount);
        Assert.NotNull(service.LastRequest);
        Assert.Equal(2, service.LastRequest!.Bills.Count);
        Assert.Equal(2, result.PersistedCount);
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

    private sealed class SpyBillPersistenceService : IBillPersistenceService
    {
        public int PersistCallCount { get; private set; }

        public BillPersistenceRequest? LastRequest { get; private set; }

        public BillPersistenceResult Persist(BillPersistenceRequest request)
        {
            PersistCallCount++;
            LastRequest = request;
            return new BillPersistenceResult(request.Bills);
        }
    }
}
