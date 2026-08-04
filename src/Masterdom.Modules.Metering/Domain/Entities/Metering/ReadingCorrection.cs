using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class ReadingCorrection : ValueObject
{
    private ReadingCorrection(
        decimal previousValue,
        decimal correctedValue,
        string reason,
        SubmittedBy correctedBy,
        DateTime correctedAtUtc)
    {
        PreviousValue = previousValue;
        CorrectedValue = correctedValue;
        Reason = reason;
        CorrectedBy = correctedBy;
        CorrectedAtUtc = correctedAtUtc;
    }

    public decimal PreviousValue { get; }

    public decimal CorrectedValue { get; }

    public string Reason { get; }

    public SubmittedBy CorrectedBy { get; }

    public DateTime CorrectedAtUtc { get; }

    public static ReadingCorrection Create(
        decimal previousValue,
        decimal correctedValue,
        string reason,
        SubmittedBy correctedBy,
        DateTime correctedAtUtc)
    {
        if (correctedValue < 0)
        {
            throw new InvalidOperationException("Corrected reading cannot be negative.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ArgumentNullException.ThrowIfNull(correctedBy);

        var normalizedReason = reason.Trim();
        if (normalizedReason.Length > 300)
        {
            throw new InvalidOperationException("Correction reason cannot exceed 300 characters.");
        }

        return new ReadingCorrection(
            previousValue,
            correctedValue,
            normalizedReason,
            correctedBy,
            DateTime.SpecifyKind(correctedAtUtc, DateTimeKind.Utc));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PreviousValue;
        yield return CorrectedValue;
        yield return Reason;
        yield return CorrectedBy;
        yield return CorrectedAtUtc;
    }
}
