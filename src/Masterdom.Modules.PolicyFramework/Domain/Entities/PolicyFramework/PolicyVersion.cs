namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

public sealed class PolicyVersion
{
    private PolicyVersion(
        int versionNumber,
        EffectiveDateRange effectiveDateRange,
        PolicyCondition condition,
        PolicyMetadata metadata,
        PolicyStatus status,
        DateTime createdAtUtc,
        DateTime? activatedAtUtc,
        DateTime? expiredAtUtc)
    {
        VersionNumber = versionNumber;
        EffectiveDateRange = effectiveDateRange;
        Condition = condition;
        Metadata = metadata;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        ActivatedAtUtc = activatedAtUtc;
        ExpiredAtUtc = expiredAtUtc;
    }

    public int VersionNumber { get; private set; }

    public EffectiveDateRange EffectiveDateRange { get; private set; }

    public PolicyCondition Condition { get; private set; }

    public PolicyMetadata Metadata { get; private set; }

    public PolicyStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? ActivatedAtUtc { get; private set; }

    public DateTime? ExpiredAtUtc { get; private set; }

    public static PolicyVersion Create(
        int versionNumber,
        EffectiveDateRange effectiveDateRange,
        PolicyCondition condition,
        PolicyMetadata metadata,
        DateTime createdAtUtc)
    {
        if (versionNumber <= 0)
        {
            throw new InvalidOperationException("Policy version number must be greater than zero.");
        }

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Policy version creation timestamp must be UTC.");
        }

        ArgumentNullException.ThrowIfNull(effectiveDateRange);
        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(metadata);

        return new PolicyVersion(
            versionNumber,
            effectiveDateRange,
            condition,
            metadata,
            PolicyStatus.Draft,
            createdAtUtc,
            null,
            null);
    }

    public PolicyVersion Activate(DateTime activatedAtUtc)
    {
        if (activatedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Policy version activation timestamp must be UTC.");
        }

        if (Status == PolicyStatus.Archived)
        {
            throw new InvalidOperationException("Archived policy versions cannot be activated.");
        }

        return new PolicyVersion(
            VersionNumber,
            EffectiveDateRange,
            Condition,
            Metadata,
            PolicyStatus.Active,
            CreatedAtUtc,
            activatedAtUtc,
            null);
    }

    public PolicyVersion Expire(DateTime expiredAtUtc)
    {
        if (expiredAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Policy version expiration timestamp must be UTC.");
        }

        return new PolicyVersion(
            VersionNumber,
            EffectiveDateRange,
            Condition,
            Metadata,
            PolicyStatus.Expired,
            CreatedAtUtc,
            ActivatedAtUtc,
            expiredAtUtc);
    }

    public PolicyVersion Archive(DateTime archivedAtUtc)
    {
        if (archivedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Policy version archive timestamp must be UTC.");
        }

        return new PolicyVersion(
            VersionNumber,
            EffectiveDateRange,
            Condition,
            Metadata,
            PolicyStatus.Archived,
            CreatedAtUtc,
            ActivatedAtUtc,
            ExpiredAtUtc ?? archivedAtUtc);
    }
}
