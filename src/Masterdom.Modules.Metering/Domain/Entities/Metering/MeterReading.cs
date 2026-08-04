using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class MeterReading : ValueObject
{
    private MeterReading(
        Guid readingId,
        ReadingDate readingDate,
        ReadingValue readingValue,
        ReadingSource readingSource,
        SubmittedBy submittedBy,
        DateTime submittedAtUtc,
        ReadingStatus readingStatus,
        ApprovalStatus approvalStatus,
        bool isRollover,
        Consumption? consumption,
        ReviewedBy? reviewedBy,
        ReviewDate? reviewDate,
        ReadingNotes? readingNotes,
        CorrectionHistory correctionHistory,
        ReadingSnapshot snapshot)
    {
        ReadingId = readingId;
        ReadingDate = readingDate;
        ReadingValue = readingValue;
        ReadingSource = readingSource;
        SubmittedBy = submittedBy;
        SubmittedAtUtc = submittedAtUtc;
        ReadingStatus = readingStatus;
        ApprovalStatus = approvalStatus;
        IsRollover = isRollover;
        Consumption = consumption;
        ReviewedBy = reviewedBy;
        ReviewDate = reviewDate;
        ReadingNotes = readingNotes;
        CorrectionHistory = correctionHistory;
        Snapshot = snapshot;
    }

    public Guid ReadingId { get; }

    public ReadingDate ReadingDate { get; }

    public ReadingValue ReadingValue { get; }

    public ReadingSource ReadingSource { get; }

    public SubmittedBy SubmittedBy { get; }

    public DateTime SubmittedAtUtc { get; }

    public ReadingStatus ReadingStatus { get; }

    public ApprovalStatus ApprovalStatus { get; }

    public bool IsRollover { get; }

    public Consumption? Consumption { get; }

    public ReviewedBy? ReviewedBy { get; }

    public ReviewDate? ReviewDate { get; }

    public ReadingNotes? ReadingNotes { get; }

    public CorrectionHistory CorrectionHistory { get; }

    public ReadingSnapshot Snapshot { get; }

    public static MeterReading Submit(
        ReadingDate readingDate,
        ReadingValue readingValue,
        ReadingSource readingSource,
        SubmittedBy submittedBy,
        DateTime submittedAtUtc,
        bool isRollover,
        ReadingNotes? readingNotes)
    {
        ArgumentNullException.ThrowIfNull(readingDate);
        ArgumentNullException.ThrowIfNull(readingValue);
        ArgumentNullException.ThrowIfNull(readingSource);
        ArgumentNullException.ThrowIfNull(submittedBy);

        var utcNow = DateTime.SpecifyKind(submittedAtUtc, DateTimeKind.Utc);

        return new MeterReading(
            Guid.CreateVersion7(),
            readingDate,
            readingValue,
            readingSource,
            submittedBy,
            utcNow,
            ReadingStatus.Submitted,
            ApprovalStatus.Pending,
            isRollover,
            null,
            null,
            null,
            readingNotes,
            CorrectionHistory.Empty,
            ReadingSnapshot.Create(utcNow, ReadingStatus.Submitted, ApprovalStatus.Pending));
    }

    public MeterReading Approve(ReviewedBy reviewedBy, ReviewDate reviewDate, Consumption consumption)
    {
        ArgumentNullException.ThrowIfNull(reviewedBy);
        ArgumentNullException.ThrowIfNull(reviewDate);
        ArgumentNullException.ThrowIfNull(consumption);

        return new MeterReading(
            ReadingId,
            ReadingDate,
            ReadingValue,
            ReadingSource,
            SubmittedBy,
            SubmittedAtUtc,
            ReadingStatus.Approved,
            ApprovalStatus.Approved,
            IsRollover,
            consumption,
            reviewedBy,
            reviewDate,
            ReadingNotes,
            CorrectionHistory,
            ReadingSnapshot.Create(reviewDate.ValueUtc, ReadingStatus.Approved, ApprovalStatus.Approved));
    }

    public MeterReading Correct(ReadingValue correctedValue, ReadingCorrection correction, Consumption? consumption)
    {
        ArgumentNullException.ThrowIfNull(correctedValue);
        ArgumentNullException.ThrowIfNull(correction);

        var nextHistory = CorrectionHistory.Add(correction);

        return new MeterReading(
            ReadingId,
            ReadingDate,
            correctedValue,
            ReadingSource,
            SubmittedBy,
            SubmittedAtUtc,
            ReadingStatus.Corrected,
            ApprovalStatus.Approved,
            IsRollover,
            consumption,
            ReviewedBy,
            ReviewDate,
            ReadingNotes,
            nextHistory,
            ReadingSnapshot.Create(correction.CorrectedAtUtc, ReadingStatus.Corrected, ApprovalStatus.Approved));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ReadingId;
        yield return ReadingDate;
        yield return ReadingValue;
        yield return ReadingSource;
        yield return SubmittedBy;
        yield return SubmittedAtUtc;
        yield return ReadingStatus;
        yield return ApprovalStatus;
        yield return IsRollover;
        yield return Consumption;
        yield return ReviewedBy;
        yield return ReviewDate;
        yield return ReadingNotes;
        yield return CorrectionHistory;
        yield return Snapshot;
    }
}
