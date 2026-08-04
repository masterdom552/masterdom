using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class ReadingSnapshot : ValueObject
{
    private ReadingSnapshot(DateTime capturedAtUtc, ReadingStatus readingStatus, ApprovalStatus approvalStatus)
    {
        CapturedAtUtc = capturedAtUtc;
        ReadingStatus = readingStatus;
        ApprovalStatus = approvalStatus;
    }

    public DateTime CapturedAtUtc { get; }

    public ReadingStatus ReadingStatus { get; }

    public ApprovalStatus ApprovalStatus { get; }

    public static ReadingSnapshot Create(DateTime capturedAtUtc, ReadingStatus readingStatus, ApprovalStatus approvalStatus)
    {
        ArgumentNullException.ThrowIfNull(readingStatus);
        ArgumentNullException.ThrowIfNull(approvalStatus);

        return new ReadingSnapshot(
            DateTime.SpecifyKind(capturedAtUtc, DateTimeKind.Utc),
            readingStatus,
            approvalStatus);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CapturedAtUtc;
        yield return ReadingStatus;
        yield return ApprovalStatus;
    }
}
