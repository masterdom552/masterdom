using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;
using Masterdom.Core.Primitives;
using Masterdom.Modules.Payment.Contracts.Billing;
using Masterdom.Modules.Payment.Domain.Entities.Payment.Events;

namespace Masterdom.Modules.Payment.Domain.Entities.Payment;

public sealed class Payment : AggregateRoot<PaymentId>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly List<PaymentAllocation> _allocations = [];
    private readonly List<PaymentVersion> _versions = [];
    private readonly List<PaymentReceipt> _receipts = [];
    private readonly List<PaymentSnapshot> _snapshots = [];

    private Payment(
        PaymentId id,
        PaymentReference paymentReference,
        PaymentAmount paymentAmount,
        PaymentDate paymentDate,
        PaymentMethod paymentMethod,
        PaymentChannel paymentChannel,
        PaymentSource paymentSource,
        DateTime receivedAtUtc)
        : base(id)
    {
        PaymentReference = paymentReference;
        PaymentAmount = paymentAmount;
        PaymentDate = paymentDate;
        PaymentMethod = paymentMethod;
        PaymentChannel = paymentChannel;
        PaymentSource = paymentSource;
        PaymentStatus = PaymentStatus.Received;
        ReceivedAtUtc = receivedAtUtc;
    }

    public PaymentReference PaymentReference { get; private set; }

    public PaymentAmount PaymentAmount { get; private set; }

    public PaymentDate PaymentDate { get; private set; }

    public PaymentMethod PaymentMethod { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }

    public PaymentChannel PaymentChannel { get; private set; }

    public PaymentSource PaymentSource { get; private set; }

    public DateTime ReceivedAtUtc { get; private set; }

    public DateTime? ReversedAtUtc { get; private set; }

    public DateTime? VoidedAtUtc { get; private set; }

    public string? ReversalReason { get; private set; }

    public string? VoidReason { get; private set; }

    public IReadOnlyCollection<PaymentAllocation> Allocations => _allocations.AsReadOnly();

    public IReadOnlyCollection<PaymentVersion> Versions => _versions.AsReadOnly();

    public IReadOnlyCollection<PaymentReceipt> Receipts => _receipts.AsReadOnly();

    public IReadOnlyCollection<PaymentSnapshot> Snapshots => _snapshots.AsReadOnly();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public PaymentVersion CurrentVersion => _versions[^1];

    public PaymentReceipt CurrentReceipt => _receipts[^1];

    public static Payment Receive(
        PaymentId id,
        PaymentReference paymentReference,
        PaymentAmount paymentAmount,
        PaymentDate paymentDate,
        PaymentMethod paymentMethod,
        PaymentChannel paymentChannel,
        PaymentSource paymentSource,
        DateTime receivedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(paymentReference);
        ArgumentNullException.ThrowIfNull(paymentAmount);
        ArgumentNullException.ThrowIfNull(paymentDate);
        ArgumentNullException.ThrowIfNull(paymentMethod);
        ArgumentNullException.ThrowIfNull(paymentChannel);
        ArgumentNullException.ThrowIfNull(paymentSource);

        if (receivedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Payment received timestamp must be UTC.");
        }

        var payment = new Payment(
            id,
            paymentReference,
            paymentAmount,
            paymentDate,
            paymentMethod,
            paymentChannel,
            paymentSource,
            receivedAtUtc);

        payment.AppendVersion("Payment received.", receivedAtUtc);
        payment.Raise(new PaymentReceivedDomainEvent(payment.Id, payment.PaymentReference.Value, payment.PaymentAmount.Value, receivedAtUtc));

        return payment;
    }

    public void Allocate(IReadOnlyCollection<BillSettlementContract> billSettlements, DateTime allocatedAtUtc)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(billSettlements);

        if (allocatedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Payment allocation timestamp must be UTC.");
        }

        if (billSettlements.Count == 0)
        {
            throw new InvalidOperationException("At least one billing settlement contract is required for allocation.");
        }

        var currentAllocated = GetAllocatedTotal();
        var requestedTotal = PaymentAmount.Create(billSettlements.Sum(x => x.AllocationAmount));
        var nextAllocated = currentAllocated.Add(requestedTotal);

        if (nextAllocated.Value > PaymentAmount.Value)
        {
            throw new InvalidOperationException("Allocation cannot exceed payment amount.");
        }

        foreach (var settlement in billSettlements)
        {
            if (settlement.OutstandingAmount < 0m)
            {
                throw new InvalidOperationException("Bill outstanding amount cannot be negative.");
            }

            if (settlement.AllocationAmount <= 0m)
            {
                throw new InvalidOperationException("Allocation amount must be greater than zero.");
            }

            if (settlement.AllocationAmount > settlement.OutstandingAmount)
            {
                throw new InvalidOperationException("Allocation amount cannot exceed bill outstanding amount.");
            }

            _allocations.Add(PaymentAllocation.Create(
                settlement.BillId,
                settlement.BillNumber,
                PaymentAmount.Create(settlement.AllocationAmount),
                settlement.DueDate,
                allocatedAtUtc));
        }

        PaymentStatus = nextAllocated.Value == PaymentAmount.Value
            ? PaymentStatus.Allocated
            : PaymentStatus.PartiallyAllocated;

        AppendVersion("Payment allocation updated.", allocatedAtUtc);
        Raise(new PaymentAllocatedDomainEvent(Id, requestedTotal.Value, billSettlements.Count, allocatedAtUtc));
    }

    public void Reverse(string reason, DateTime reversedAtUtc)
    {
        EnsureMutable();
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (reversedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Payment reversal timestamp must be UTC.");
        }

        for (var i = 0; i < _allocations.Count; i++)
        {
            _allocations[i] = _allocations[i].Reverse(reason, reversedAtUtc);
        }

        PaymentStatus = PaymentStatus.Reversed;
        ReversedAtUtc = reversedAtUtc;
        ReversalReason = reason.Trim();

        AppendVersion("Payment reversed.", reversedAtUtc);
        Raise(new PaymentReversedDomainEvent(Id, ReversalReason, reversedAtUtc));
    }

    public void Void(string reason, DateTime voidedAtUtc)
    {
        EnsureMutable();
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (voidedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Payment void timestamp must be UTC.");
        }

        if (_allocations.Any(x => !x.IsReversed))
        {
            throw new InvalidOperationException("Allocated payments must be reversed before they can be voided.");
        }

        PaymentStatus = PaymentStatus.Voided;
        VoidedAtUtc = voidedAtUtc;
        VoidReason = reason.Trim();

        AppendVersion("Payment voided.", voidedAtUtc);
        Raise(new PaymentVoidedDomainEvent(Id, VoidReason, voidedAtUtc));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void EnsureMutable()
    {
        if (PaymentStatus == PaymentStatus.Reversed)
        {
            throw new InvalidOperationException("Reversed payments are immutable.");
        }

        if (PaymentStatus == PaymentStatus.Voided)
        {
            throw new InvalidOperationException("Voided payments are immutable.");
        }
    }

    private PaymentAmount GetAllocatedTotal()
    {
        var total = _allocations
            .Where(x => !x.IsReversed)
            .Sum(x => x.Amount.Value);

        return PaymentAmount.Create(total);
    }

    private void AppendVersion(string changeReason, DateTime occurredAtUtc)
    {
        var versionNumber = _versions.Count + 1;
        var version = PaymentVersion.Create(versionNumber, PaymentAmount, PaymentStatus, changeReason, occurredAtUtc);
        var receipt = PaymentReceipt.Generate(versionNumber, PaymentAmount, PaymentStatus, occurredAtUtc);
        var allocatedAmount = GetAllocatedTotal();
        var snapshot = PaymentSnapshot.Capture(
            versionNumber,
            PaymentStatus,
            PaymentAmount,
            allocatedAmount,
            PaymentAmount.Subtract(allocatedAmount),
            _allocations.ToList().AsReadOnly(),
            receipt.ReceiptNumber,
            occurredAtUtc);

        _versions.Add(version);
        _receipts.Add(receipt);
        _snapshots.Add(snapshot);

        Raise(new PaymentVersionCreatedDomainEvent(Id, versionNumber, changeReason, occurredAtUtc));
        Raise(new ReceiptGeneratedDomainEvent(Id, receipt.ReceiptNumber, versionNumber, occurredAtUtc));
    }

    private void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
