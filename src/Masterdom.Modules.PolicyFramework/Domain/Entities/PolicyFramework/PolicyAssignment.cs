namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

public sealed class PolicyAssignment
{
    private PolicyAssignment(
        Guid assignmentId,
        PolicyScope scope,
        string assignedEntityType,
        string assignedEntityId,
        EffectiveDateRange effectiveDateRange,
        DateTime assignedAtUtc)
    {
        AssignmentId = assignmentId;
        Scope = scope;
        AssignedEntityType = assignedEntityType;
        AssignedEntityId = assignedEntityId;
        EffectiveDateRange = effectiveDateRange;
        AssignedAtUtc = assignedAtUtc;
    }

    public Guid AssignmentId { get; private set; }

    public PolicyScope Scope { get; private set; }

    public string AssignedEntityType { get; private set; }

    public string AssignedEntityId { get; private set; }

    public EffectiveDateRange EffectiveDateRange { get; private set; }

    public DateTime AssignedAtUtc { get; private set; }

    public static PolicyAssignment Create(
        Guid assignmentId,
        PolicyScope scope,
        string assignedEntityType,
        string assignedEntityId,
        EffectiveDateRange effectiveDateRange,
        DateTime assignedAtUtc)
    {
        if (assignmentId == Guid.Empty)
        {
            throw new InvalidOperationException("Policy assignment identifier cannot be empty.");
        }

        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(assignedEntityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(assignedEntityId);
        ArgumentNullException.ThrowIfNull(effectiveDateRange);

        if (assignedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Policy assignment timestamp must be UTC.");
        }

        return new PolicyAssignment(
            assignmentId,
            scope,
            assignedEntityType.Trim(),
            assignedEntityId.Trim(),
            effectiveDateRange,
            assignedAtUtc);
    }

    public bool AppliesTo(PolicyScope requestedScope, DateOnly asOfDate)
    {
        ArgumentNullException.ThrowIfNull(requestedScope);
        return Scope.AppliesTo(requestedScope) && EffectiveDateRange.Contains(asOfDate);
    }
}
