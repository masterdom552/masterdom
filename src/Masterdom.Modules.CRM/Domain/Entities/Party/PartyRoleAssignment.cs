using Masterdom.Core.Primitives;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Represents a role assignment owned by a party.
/// </summary>
public sealed class PartyRoleAssignment : Entity<PartyRoleAssignmentId>
{
    private PartyRoleAssignment(
        PartyRoleAssignmentId id,
        PartyRoleType roleType,
        DateTime assignedAtUtc,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc,
        string? assignmentReason,
        PartyRoleAssignmentStatus status,
        DateTime? deactivatedAtUtc,
        string? deactivationReason,
        DateTime? removedAtUtc,
        string? removalReason,
        DateTime? reactivatedAtUtc,
        string? reactivationReason)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(roleType);

        AssignedAtUtc = EnsureUtc(assignedAtUtc, nameof(assignedAtUtc));
        EffectiveFromUtc = EnsureUtc(effectiveFromUtc, nameof(effectiveFromUtc));

        if (effectiveToUtc.HasValue)
        {
            EffectiveToUtc = EnsureUtc(effectiveToUtc.Value, nameof(effectiveToUtc));
            if (EffectiveToUtc.Value < EffectiveFromUtc)
            {
                throw new InvalidOperationException("EffectiveToUtc cannot be earlier than EffectiveFromUtc.");
            }
        }

        RoleType = roleType;
        AssignmentReason = NormalizeOptional(assignmentReason);
        Status = status;
        DeactivatedAtUtc = deactivatedAtUtc;
        DeactivationReason = NormalizeOptional(deactivationReason);
        RemovedAtUtc = removedAtUtc;
        RemovalReason = NormalizeOptional(removalReason);
        ReactivatedAtUtc = reactivatedAtUtc;
        ReactivationReason = NormalizeOptional(reactivationReason);
    }

    public PartyRoleType RoleType { get; }

    public DateTime AssignedAtUtc { get; }

    public DateTime EffectiveFromUtc { get; }

    public DateTime? EffectiveToUtc { get; }

    public string? AssignmentReason { get; }

    public PartyRoleAssignmentStatus Status { get; private set; }

    public DateTime? DeactivatedAtUtc { get; private set; }

    public string? DeactivationReason { get; private set; }

    public DateTime? RemovedAtUtc { get; private set; }

    public string? RemovalReason { get; private set; }

    public DateTime? ReactivatedAtUtc { get; private set; }

    public string? ReactivationReason { get; private set; }

    public static PartyRoleAssignment Create(
        PartyRoleType roleType,
        DateTime assignedAtUtc,
        DateTime? effectiveFromUtc = null,
        DateTime? effectiveToUtc = null,
        string? assignmentReason = null)
    {
        ArgumentNullException.ThrowIfNull(roleType);

        var normalizedAssignedAtUtc = EnsureUtc(assignedAtUtc, nameof(assignedAtUtc));
        var normalizedEffectiveFromUtc = effectiveFromUtc.HasValue
            ? EnsureUtc(effectiveFromUtc.Value, nameof(effectiveFromUtc))
            : normalizedAssignedAtUtc;

        return new PartyRoleAssignment(
            PartyRoleAssignmentId.New(),
            roleType,
            normalizedAssignedAtUtc,
            normalizedEffectiveFromUtc,
            effectiveToUtc,
            assignmentReason,
            PartyRoleAssignmentStatus.Active,
            deactivatedAtUtc: null,
            deactivationReason: null,
            removedAtUtc: null,
            removalReason: null,
            reactivatedAtUtc: null,
            reactivationReason: null);
    }

    public bool MatchesActiveRoleType(PartyRoleType roleType, DateTime asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(roleType);
        var normalizedAsOfUtc = EnsureUtc(asOfUtc, nameof(asOfUtc));

        return RoleType == roleType
            && Status == PartyRoleAssignmentStatus.Active
            && IsEffectiveAt(normalizedAsOfUtc);
    }

    public bool IsEffectiveAt(DateTime asOfUtc)
    {
        var normalizedAsOfUtc = EnsureUtc(asOfUtc, nameof(asOfUtc));

        if (normalizedAsOfUtc < EffectiveFromUtc)
        {
            return false;
        }

        if (EffectiveToUtc.HasValue && normalizedAsOfUtc > EffectiveToUtc.Value)
        {
            return false;
        }

        return true;
    }

    public bool OverlapsWith(DateTime effectiveFromUtc, DateTime? effectiveToUtc)
    {
        var candidateStart = EnsureUtc(effectiveFromUtc, nameof(effectiveFromUtc));
        var candidateEnd = effectiveToUtc.HasValue
            ? EnsureUtc(effectiveToUtc.Value, nameof(effectiveToUtc))
            : (DateTime?)null;

        var currentEnd = EffectiveToUtc ?? DateTime.MaxValue;
        var otherEnd = candidateEnd ?? DateTime.MaxValue;

        return candidateStart <= currentEnd && EffectiveFromUtc <= otherEnd;
    }

    public void Deactivate(DateTime deactivatedAtUtc, string? reason = null)
    {
        if (Status == PartyRoleAssignmentStatus.Removed)
        {
            throw new InvalidOperationException("A removed party role assignment cannot be deactivated.");
        }

        if (Status == PartyRoleAssignmentStatus.Inactive)
        {
            return;
        }

        Status = PartyRoleAssignmentStatus.Inactive;
        DeactivatedAtUtc = EnsureUtc(deactivatedAtUtc, nameof(deactivatedAtUtc));
        DeactivationReason = NormalizeOptional(reason);
    }

    public void Reactivate(DateTime reactivatedAtUtc, string? reason = null)
    {
        if (Status == PartyRoleAssignmentStatus.Removed)
        {
            throw new InvalidOperationException("A removed party role assignment cannot be reactivated.");
        }

        var normalizedReactivatedAtUtc = EnsureUtc(reactivatedAtUtc, nameof(reactivatedAtUtc));

        if (EffectiveToUtc.HasValue && normalizedReactivatedAtUtc > EffectiveToUtc.Value)
        {
            throw new InvalidOperationException("An expired role assignment cannot be reactivated.");
        }

        Status = PartyRoleAssignmentStatus.Active;
        ReactivatedAtUtc = normalizedReactivatedAtUtc;
        ReactivationReason = NormalizeOptional(reason);
    }

    public void Remove(DateTime removedAtUtc, string? reason = null)
    {
        if (Status == PartyRoleAssignmentStatus.Removed)
        {
            return;
        }

        Status = PartyRoleAssignmentStatus.Removed;
        RemovedAtUtc = EnsureUtc(removedAtUtc, nameof(removedAtUtc));
        RemovalReason = NormalizeOptional(reason);
    }

    private static DateTime EnsureUtc(DateTime value, string paramName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException($"{paramName} must be in UTC.");
        }

        return value;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
