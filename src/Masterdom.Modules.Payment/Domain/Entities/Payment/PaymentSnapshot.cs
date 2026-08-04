namespace Masterdom.Modules.Payment.Domain.Entities.Payment;

public sealed class PaymentSnapshot
{
    private PaymentSnapshot(
        Guid snapshotId,
        int versionNumber,
        PaymentStatus paymentStatus,
        PaymentAmount paymentAmount,
        PaymentAmount allocatedAmount,
        PaymentAmount unallocatedAmount,
        IReadOnlyList<PaymentAllocation> allocations,
        string receiptNumber,
        DateTime capturedAtUtc)
    {
        SnapshotId = snapshotId;
        VersionNumber = versionNumber;
        PaymentStatus = paymentStatus;
        PaymentAmount = paymentAmount;
        AllocatedAmount = allocatedAmount;
        UnallocatedAmount = unallocatedAmount;
        Allocations = allocations;
        ReceiptNumber = receiptNumber;
        CapturedAtUtc = capturedAtUtc;
    }

    public Guid SnapshotId { get; private set; }

    public int VersionNumber { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }

    public PaymentAmount PaymentAmount { get; private set; }

    public PaymentAmount AllocatedAmount { get; private set; }

    public PaymentAmount UnallocatedAmount { get; private set; }

    public IReadOnlyList<PaymentAllocation> Allocations { get; private set; }

    public string ReceiptNumber { get; private set; }

    public DateTime CapturedAtUtc { get; private set; }

    public static PaymentSnapshot Capture(
        int versionNumber,
        PaymentStatus paymentStatus,
        PaymentAmount paymentAmount,
        PaymentAmount allocatedAmount,
        PaymentAmount unallocatedAmount,
        IReadOnlyList<PaymentAllocation> allocations,
        string receiptNumber,
        DateTime capturedAtUtc)
    {
        if (versionNumber <= 0)
        {
            throw new InvalidOperationException("Snapshot version number must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(paymentStatus);
        ArgumentNullException.ThrowIfNull(paymentAmount);
        ArgumentNullException.ThrowIfNull(allocatedAmount);
        ArgumentNullException.ThrowIfNull(unallocatedAmount);
        ArgumentNullException.ThrowIfNull(allocations);
        ArgumentException.ThrowIfNullOrWhiteSpace(receiptNumber);

        if (capturedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Payment snapshot timestamp must be UTC.");
        }

        return new PaymentSnapshot(
            Guid.CreateVersion7(),
            versionNumber,
            paymentStatus,
            paymentAmount,
            allocatedAmount,
            unallocatedAmount,
            allocations.ToList().AsReadOnly(),
            receiptNumber.Trim(),
            capturedAtUtc);
    }
}
