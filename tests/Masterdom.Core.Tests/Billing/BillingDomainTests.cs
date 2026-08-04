using Masterdom.Core.Identifiers;
using Masterdom.Core.Financial.ValueObjects;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using Masterdom.Modules.Billing.Domain.Entities.Billing.Events;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Core.Tests.Billing;

public sealed class BillingDomainTests
{
    [Fact]
    public void Generate_ShouldInitializeGeneratedBillAndRaiseGeneratedEvent()
    {
        var bill = CreateBill();

        Assert.Equal(BillStatus.Generated, bill.Status);
        Assert.Single(bill.Versions);
        Assert.Contains(bill.DomainEvents, x => x is BillGeneratedDomainEvent);
    }

    [Fact]
    public void AddAdjustment_ShouldCreateNewVersionAndRaiseRecalculationEvents()
    {
        var bill = CreateBill();

        bill.AddAdjustment(
            AdjustmentLine.Create(AdjustmentKind.Debit, "Late fee", 25m),
            GeneratedDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            IssueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))));

        Assert.Equal(2, bill.Versions.Count);
        Assert.Equal(125m, bill.CurrentSnapshot.OutstandingAmount.Value);
        Assert.Contains(bill.DomainEvents, x => x is AdjustmentAddedDomainEvent);
        Assert.Contains(bill.DomainEvents, x => x is BillRecalculatedDomainEvent);
    }

    [Fact]
    public void ApplyCredit_ShouldCreateNewVersionAndReduceOutstandingAmount()
    {
        var bill = CreateBill();

        bill.ApplyCredit(
            CreditLine.Create("Credit note", 20m, "CR-1"),
            GeneratedDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            IssueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))));

        Assert.Equal(2, bill.Versions.Count);
        Assert.Equal(80m, bill.CurrentSnapshot.OutstandingAmount.Value);
        Assert.Contains(bill.DomainEvents, x => x is CreditAppliedDomainEvent);
        Assert.Contains(bill.DomainEvents, x => x is BillRecalculatedDomainEvent);
    }

    [Fact]
    public void Generate_ShouldAssignSnapshotCurrency_AndPreserveOnRecalculation()
    {
        var bill = CreateBill();

        Assert.Equal("USD", bill.CurrentSnapshot.Currency.Code);

        bill.AddAdjustment(
            AdjustmentLine.Create(AdjustmentKind.Debit, "Late fee", 25m),
            GeneratedDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            IssueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))));

        Assert.Equal("USD", bill.CurrentSnapshot.Currency.Code);
    }

    [Fact]
    public void Finalize_ShouldPreventFurtherMutations()
    {
        var bill = CreateBill();
        bill.FinalizeBill();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            bill.AddAdjustment(
                AdjustmentLine.Create(AdjustmentKind.Debit, "Late fee", 25m),
                GeneratedDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
                IssueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
                DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)))));

        Assert.Equal("Finalized bill cannot be modified.", exception.Message);
        Assert.Contains(bill.DomainEvents, x => x is BillFinalizedDomainEvent);
    }

    [Fact]
    public void Void_ShouldPreventFurtherMutations()
    {
        var bill = CreateBill();
        bill.Void("Operator correction");

        var exception = Assert.Throws<InvalidOperationException>(() => bill.FinalizeBill());

        Assert.Equal("Voided bill cannot be modified.", exception.Message);
        Assert.Contains(bill.DomainEvents, x => x is BillVoidedDomainEvent);
    }

    private static BillAggregate CreateBill()
    {
        return BillAggregate.Generate(
            BillId.New(),
            BillNumber.Create("BILL-1001"),
            TenancyReference.Create(Guid.NewGuid()),
            LeaseReference.Create(Guid.NewGuid()),
            PropertyReference.Create(Guid.NewGuid()),
            PersonReference.Create(PersonId.New()),
            BillingPeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow.Date),
                DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(1))),
            BillingCycle.Monthly,
            GeneratedDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            IssueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))),
            Currency.Create("USD"),
            ChargeCollection.Create([
                ChargeLine.Create(ChargeKind.Rent, "Base rent", 100m)
            ]));
    }
}
