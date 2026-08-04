namespace Masterdom.Modules.Payment.Domain.Entities.Payment;

public sealed class PaymentReceipt
{
    private PaymentReceipt(
        Guid receiptId,
        string receiptNumber,
        int versionNumber,
        PaymentAmount amount,
        PaymentStatus paymentStatus,
        DateTime issuedAtUtc)
    {
        ReceiptId = receiptId;
        ReceiptNumber = receiptNumber;
        VersionNumber = versionNumber;
        Amount = amount;
        PaymentStatus = paymentStatus;
        IssuedAtUtc = issuedAtUtc;
    }

    public Guid ReceiptId { get; private set; }

    public string ReceiptNumber { get; private set; }

    public int VersionNumber { get; private set; }

    public PaymentAmount Amount { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }

    public DateTime IssuedAtUtc { get; private set; }

    public static PaymentReceipt Generate(int versionNumber, PaymentAmount amount, PaymentStatus paymentStatus, DateTime issuedAtUtc)
    {
        if (versionNumber <= 0)
        {
            throw new InvalidOperationException("Receipt version number must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(amount);
        ArgumentNullException.ThrowIfNull(paymentStatus);

        if (issuedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Receipt issuance timestamp must be UTC.");
        }

        var receiptNumber = $"PMT-{issuedAtUtc:yyyyMMddHHmmss}-{versionNumber}";

        return new PaymentReceipt(
            Guid.CreateVersion7(),
            receiptNumber,
            versionNumber,
            amount,
            paymentStatus,
            issuedAtUtc);
    }
}
