namespace Masterdom.Modules.Payment.Domain.Entities.Payment;

public sealed class PaymentVersion
{
    private PaymentVersion(
        int versionNumber,
        PaymentAmount paymentAmount,
        PaymentStatus paymentStatus,
        string changeReason,
        DateTime createdAtUtc)
    {
        VersionNumber = versionNumber;
        PaymentAmount = paymentAmount;
        PaymentStatus = paymentStatus;
        ChangeReason = changeReason;
        CreatedAtUtc = createdAtUtc;
    }

    public int VersionNumber { get; private set; }

    public PaymentAmount PaymentAmount { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }

    public string ChangeReason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static PaymentVersion Create(
        int versionNumber,
        PaymentAmount paymentAmount,
        PaymentStatus paymentStatus,
        string changeReason,
        DateTime createdAtUtc)
    {
        if (versionNumber <= 0)
        {
            throw new InvalidOperationException("Payment version number must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(paymentAmount);
        ArgumentNullException.ThrowIfNull(paymentStatus);
        ArgumentException.ThrowIfNullOrWhiteSpace(changeReason);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Payment version timestamp must be UTC.");
        }

        return new PaymentVersion(
            versionNumber,
            paymentAmount,
            paymentStatus,
            changeReason.Trim(),
            createdAtUtc);
    }
}
