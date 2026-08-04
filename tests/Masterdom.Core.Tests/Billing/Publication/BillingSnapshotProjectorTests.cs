using Masterdom.Core.Financial.ValueObjects;
using Masterdom.Core.Identifiers;
using Masterdom.Modules.Billing.Application.Publication;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Core.Tests.Billing.MonthlyBilling.Publication;

public sealed class BillingSnapshotProjectorTests
{
    [Fact]
    public void Project_ShouldPreserveSnapshotFacts_AndCurrencyCode()
    {
        var bill = CreateBill("BILL-SNAPSHOT-001", "USD", 1000m);
        var projector = new BillingSnapshotProjector();

        var model = projector.Project(bill, "corr-snapshot-001");

        Assert.Equal(bill.Id.Value, model.BillId);
        Assert.Equal(bill.BillNumber.Value, model.BillNumber);
        Assert.Equal(bill.CurrentSnapshot.BillingPeriod.StartDate, model.BillingPeriodStartDate);
        Assert.Equal(bill.CurrentSnapshot.BillingPeriod.EndDate, model.BillingPeriodEndDate);
        Assert.Equal(bill.PropertyReference.PropertyId, model.PropertyId);
        Assert.Equal(bill.TenancyReference.TenancyId, model.TenancyId);
        Assert.Equal(bill.LeaseReference.LeaseId, model.LeaseId);
        Assert.Equal(bill.CurrentSnapshot.IssueDate.Value, model.IssueDate);
        Assert.Equal(bill.CurrentSnapshot.DueDate.Value, model.DueDate);
        Assert.Equal("USD", model.CurrencyCode);
        Assert.Equal(bill.CurrentSnapshot.TotalAmount.Value, model.TotalAmount);
        Assert.Equal(bill.CurrentSnapshot.OutstandingAmount.Value, model.OutstandingAmount);
        Assert.Equal("corr-snapshot-001", model.CorrelationId);
        Assert.Single(model.ChargeLines);
    }

    [Fact]
    public void Project_ShouldPreserveChargeLineValues()
    {
        var bill = CreateBill("BILL-SNAPSHOT-002", "USD", 1250m);
        var projector = new BillingSnapshotProjector();

        var model = projector.Project(bill);
        var line = Assert.Single(model.ChargeLines);

        Assert.Equal("Rent", line.ChargeCategory);
        Assert.Equal("Rent charge", line.Description);
        Assert.Equal(1250m, line.Amount);
        Assert.Equal("LEASE-001", line.ExternalReference);
    }

    private static BillAggregate CreateBill(string billNumber, string currencyCode, decimal amount)
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
            Currency.Create(currencyCode),
            ChargeCollection.Create([ChargeLine.Create(ChargeKind.Rent, "Rent charge", amount, "LEASE-001")]));
    }
}
