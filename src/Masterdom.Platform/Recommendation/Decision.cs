namespace Masterdom.Platform.Recommendation;

public sealed class Decision
{
    private Decision(
        DecisionId id,
        DecisionType type,
        DecisionStatus status,
        DecisionReason reason,
        RecommendationBundleId bundleId,
        DateTime createdAtUtc,
        DateTime? decidedAtUtc,
        DateTime? appliedAtUtc)
    {
        Id = id;
        Type = type;
        Status = status;
        Reason = reason;
        BundleId = bundleId;
        CreatedAtUtc = createdAtUtc;
        DecidedAtUtc = decidedAtUtc;
        AppliedAtUtc = appliedAtUtc;
    }

    public DecisionId Id { get; }

    public DecisionType Type { get; }

    public DecisionStatus Status { get; }

    public DecisionReason Reason { get; }

    public RecommendationBundleId BundleId { get; }

    public DateTime CreatedAtUtc { get; }

    public DateTime? DecidedAtUtc { get; }

    public DateTime? AppliedAtUtc { get; }

    public static Decision CreatePending(
        DecisionId id,
        DecisionType type,
        DecisionReason reason,
        RecommendationBundleId bundleId,
        DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(reason);
        ArgumentNullException.ThrowIfNull(bundleId);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Decision createdAtUtc must be UTC.");
        }

        return new Decision(
            id,
            type,
            DecisionStatus.Pending,
            reason,
            bundleId,
            createdAtUtc,
            decidedAtUtc: null,
            appliedAtUtc: null);
    }

    public Decision Approve(DateTime decidedAtUtc)
    {
        EnsureUtc(decidedAtUtc, nameof(decidedAtUtc));

        if (Status != DecisionStatus.Pending)
        {
            throw new InvalidOperationException("Only pending decisions can be approved.");
        }

        return new Decision(
            Id,
            Type,
            DecisionStatus.Approved,
            Reason,
            BundleId,
            CreatedAtUtc,
            decidedAtUtc,
            AppliedAtUtc);
    }

    public Decision Reject(DateTime decidedAtUtc)
    {
        EnsureUtc(decidedAtUtc, nameof(decidedAtUtc));

        if (Status != DecisionStatus.Pending)
        {
            throw new InvalidOperationException("Only pending decisions can be rejected.");
        }

        return new Decision(
            Id,
            Type,
            DecisionStatus.Rejected,
            Reason,
            BundleId,
            CreatedAtUtc,
            decidedAtUtc,
            AppliedAtUtc);
    }

    public Decision Apply(DateTime appliedAtUtc)
    {
        EnsureUtc(appliedAtUtc, nameof(appliedAtUtc));

        if (Status != DecisionStatus.Approved)
        {
            throw new InvalidOperationException("Only approved decisions can be applied.");
        }

        return new Decision(
            Id,
            Type,
            DecisionStatus.Applied,
            Reason,
            BundleId,
            CreatedAtUtc,
            DecidedAtUtc,
            appliedAtUtc);
    }

    private static void EnsureUtc(DateTime value, string argument)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException($"{argument} must be UTC.");
        }
    }
}
