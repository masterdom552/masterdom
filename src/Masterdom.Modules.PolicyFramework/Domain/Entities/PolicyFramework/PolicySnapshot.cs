namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

public sealed class PolicySnapshot
{
    private PolicySnapshot(
        Guid snapshotId,
        int versionNumber,
        PolicyStatus policyStatus,
        EffectiveDateRange effectiveDateRange,
        PolicyCondition condition,
        PolicyMetadata metadata,
        DateTime capturedAtUtc)
    {
        SnapshotId = snapshotId;
        VersionNumber = versionNumber;
        PolicyStatus = policyStatus;
        EffectiveDateRange = effectiveDateRange;
        Condition = condition;
        Metadata = metadata;
        CapturedAtUtc = capturedAtUtc;
    }

    public Guid SnapshotId { get; private set; }

    public int VersionNumber { get; private set; }

    public PolicyStatus PolicyStatus { get; private set; }

    public EffectiveDateRange EffectiveDateRange { get; private set; }

    public PolicyCondition Condition { get; private set; }

    public PolicyMetadata Metadata { get; private set; }

    public DateTime CapturedAtUtc { get; private set; }

    public static PolicySnapshot Capture(PolicyVersion version, DateTime capturedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(version);

        if (capturedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Policy snapshot timestamp must be UTC.");
        }

        return new PolicySnapshot(
            Guid.CreateVersion7(),
            version.VersionNumber,
            version.Status,
            version.EffectiveDateRange,
            version.Condition,
            version.Metadata,
            capturedAtUtc);
    }
}
