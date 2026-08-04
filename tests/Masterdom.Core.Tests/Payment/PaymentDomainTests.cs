using Masterdom.Modules.Payment.Contracts.Billing;
using Masterdom.Modules.Payment.Domain.Entities.Payment;
using Masterdom.Modules.Payment.Domain.Entities.Payment.Events;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;

namespace Masterdom.Core.Tests.Payment;

public sealed class PaymentDomainTests
{
    [Fact]
    public void Receive_ShouldCreateInitialVersionReceiptAndEvent()
    {
        var payment = CreateReceivedPayment();

        Assert.Equal(PaymentStatus.Received, payment.PaymentStatus);
        Assert.Single(payment.Versions);
        Assert.Single(payment.Receipts);
        Assert.Single(payment.Snapshots);
        Assert.Contains(payment.DomainEvents, x => x is PaymentReceivedDomainEvent);
        Assert.Contains(payment.DomainEvents, x => x is ReceiptGeneratedDomainEvent);
        Assert.Contains(payment.DomainEvents, x => x is PaymentVersionCreatedDomainEvent);
    }

    [Fact]
    public void Allocate_ShouldAllowPartialAllocation_WithoutExceedingPaymentAmount()
    {
        var payment = CreateReceivedPayment();

        payment.Allocate(
        [
            new BillSettlementContract(Guid.NewGuid(), "BILL-001", 250m, DateOnly.FromDateTime(DateTime.UtcNow.Date), 100m)
        ],
        DateTime.UtcNow);

        Assert.Equal(PaymentStatus.PartiallyAllocated, payment.PaymentStatus);
        Assert.Single(payment.Allocations);
        Assert.Equal(2, payment.Versions.Count);
        Assert.Contains(payment.DomainEvents, x => x is PaymentAllocatedDomainEvent);
    }

    [Fact]
    public void Allocate_ShouldRejectAllocationGreaterThanPaymentAmount()
    {
        var payment = CreateReceivedPayment();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            payment.Allocate(
            [
                new BillSettlementContract(Guid.NewGuid(), "BILL-001", 1000m, DateOnly.FromDateTime(DateTime.UtcNow.Date), 600m)
            ],
            DateTime.UtcNow));

        Assert.Equal("Allocation cannot exceed payment amount.", exception.Message);
    }

    [Fact]
    public void Reverse_ShouldPreserveHistoryAndCreateNewVersion()
    {
        var payment = CreateAllocatedPayment();

        payment.Reverse("Allocation correction", DateTime.UtcNow);

        Assert.Equal(PaymentStatus.Reversed, payment.PaymentStatus);
        Assert.All(payment.Allocations, x => Assert.True(x.IsReversed));
        Assert.Equal(3, payment.Versions.Count);
        Assert.Contains(payment.DomainEvents, x => x is PaymentReversedDomainEvent);
    }

    [Fact]
    public void Void_ShouldRequireAllocationsToBeReversedFirst()
    {
        var payment = CreateAllocatedPayment();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            payment.Void("Should fail", DateTime.UtcNow));

        Assert.Equal("Allocated payments must be reversed before they can be voided.", exception.Message);
    }

    [Fact]
    public void NegativeAmounts_ShouldBeRejected()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => PaymentAmount.Create(-1m));
        Assert.Equal("Payment amount cannot be negative.", exception.Message);
    }

    private static PaymentAggregate CreateReceivedPayment()
    {
        return PaymentAggregate.Receive(
            PaymentId.New(),
            PaymentReference.Create("PAY-001"),
            PaymentAmount.Create(500m),
            PaymentDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date)),
            PaymentMethod.BankTransfer,
            PaymentChannel.Counter,
            PaymentSource.Tenant,
            DateTime.UtcNow);
    }

    private static PaymentAggregate CreateAllocatedPayment()
    {
        var payment = CreateReceivedPayment();

        payment.Allocate(
        [
            new BillSettlementContract(Guid.NewGuid(), "BILL-001", 400m, DateOnly.FromDateTime(DateTime.UtcNow.Date), 200m)
        ],
        DateTime.UtcNow);

        return payment;
    }
}
