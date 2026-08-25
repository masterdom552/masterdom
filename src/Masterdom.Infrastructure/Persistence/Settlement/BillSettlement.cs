namespace Masterdom.Infrastructure.Persistence.Settlement;

public sealed class BillSettlement
{
    private BillSettlement() { }

    public Guid Id { get; private set; }
    public Guid AllocationId { get; private set; }
    public Guid BillId { get; private set; }
    public string BillNumber { get; private set; } = string.Empty;
    public Guid PaymentId { get; private set; }
    public string PaymentReference { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateTime AllocatedAtUtc { get; private set; }
    public bool IsReversed { get; private set; }
    public DateTime? ReversedAtUtc { get; private set; }
    public string? ReversalReason { get; private set; }

    public static BillSettlement Create(
        Guid allocationId,
        Guid billId,
        string billNumber,
        Guid paymentId,
        string paymentReference,
        decimal amount,
        DateTime allocatedAtUtc)
    {
        return new BillSettlement
        {
            Id = Guid.CreateVersion7(),
            AllocationId = allocationId,
            BillId = billId,
            BillNumber = billNumber,
            PaymentId = paymentId,
            PaymentReference = paymentReference,
            Amount = amount,
            AllocatedAtUtc = allocatedAtUtc,
            IsReversed = false
        };
    }

    public void Reverse(string reason, DateTime reversedAtUtc)
    {
        IsReversed = true;
        ReversedAtUtc = reversedAtUtc;
        ReversalReason = reason;
    }
}
