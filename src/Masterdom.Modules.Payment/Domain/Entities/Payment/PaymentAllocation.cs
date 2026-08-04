namespace Masterdom.Modules.Payment.Domain.Entities.Payment;

public sealed class PaymentAllocation
{
    private PaymentAllocation(
        Guid allocationId,
        Guid billId,
        string billNumber,
        PaymentAmount amount,
        DateOnly dueDate,
        DateTime allocatedAtUtc,
        bool isReversed,
        DateTime? reversedAtUtc,
        string? reversalReason)
    {
        AllocationId = allocationId;
        BillId = billId;
        BillNumber = billNumber;
        Amount = amount;
        DueDate = dueDate;
        AllocatedAtUtc = allocatedAtUtc;
        IsReversed = isReversed;
        ReversedAtUtc = reversedAtUtc;
        ReversalReason = reversalReason;
    }

    public Guid AllocationId { get; private set; }

    public Guid BillId { get; private set; }

    public string BillNumber { get; private set; }

    public PaymentAmount Amount { get; private set; }

    public DateOnly DueDate { get; private set; }

    public DateTime AllocatedAtUtc { get; private set; }

    public bool IsReversed { get; private set; }

    public DateTime? ReversedAtUtc { get; private set; }

    public string? ReversalReason { get; private set; }

    public static PaymentAllocation Create(
        Guid billId,
        string billNumber,
        PaymentAmount amount,
        DateOnly dueDate,
        DateTime allocatedAtUtc)
    {
        if (billId == Guid.Empty)
        {
            throw new InvalidOperationException("Allocation bill identifier cannot be empty.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(billNumber);
        ArgumentNullException.ThrowIfNull(amount);

        if (allocatedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Payment allocation timestamp must be UTC.");
        }

        return new PaymentAllocation(
            Guid.CreateVersion7(),
            billId,
            billNumber.Trim(),
            amount,
            dueDate,
            allocatedAtUtc,
            false,
            null,
            null);
    }

    public PaymentAllocation Reverse(string reason, DateTime reversedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (reversedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Payment allocation reversal timestamp must be UTC.");
        }

        if (IsReversed)
        {
            return this;
        }

        return new PaymentAllocation(
            AllocationId,
            BillId,
            BillNumber,
            Amount,
            DueDate,
            AllocatedAtUtc,
            true,
            reversedAtUtc,
            reason.Trim());
    }
}
