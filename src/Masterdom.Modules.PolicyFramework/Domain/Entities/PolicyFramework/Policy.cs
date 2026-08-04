using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;
using Masterdom.Core.Primitives;
using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Events;

namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

public sealed class Policy : AggregateRoot<PolicyId>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly List<PolicyVersion> _versions = [];
    private readonly List<PolicyAssignment> _assignments = [];
    private readonly List<PolicySnapshot> _snapshots = [];

    private Policy(
        PolicyId id,
        PolicyType policyType,
        PolicyCategory policyCategory,
        PolicyReference policyReference,
        PolicyScope scope,
        DateTime createdAtUtc)
        : base(id)
    {
        PolicyType = policyType;
        PolicyCategory = policyCategory;
        PolicyReference = policyReference;
        Scope = scope;
        Status = PolicyStatus.Draft;
        CreatedAtUtc = createdAtUtc;
    }

    public PolicyType PolicyType { get; private set; }

    public PolicyCategory PolicyCategory { get; private set; }

    public PolicyReference PolicyReference { get; private set; }

    public PolicyScope Scope { get; private set; }

    public PolicyStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? ActivatedAtUtc { get; private set; }

    public DateTime? ExpiredAtUtc { get; private set; }

    public DateTime? ArchivedAtUtc { get; private set; }

    public string? ArchivedReason { get; private set; }

    public PolicyVersion CurrentVersion => _versions
        .OrderByDescending(x => x.VersionNumber)
        .First();

    public IReadOnlyCollection<PolicyVersion> Versions => _versions.AsReadOnly();

    public IReadOnlyCollection<PolicyAssignment> Assignments => _assignments.AsReadOnly();

    public IReadOnlyCollection<PolicySnapshot> Snapshots => _snapshots.AsReadOnly();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Policy Create(
        PolicyId id,
        PolicyType policyType,
        PolicyCategory policyCategory,
        PolicyReference policyReference,
        PolicyScope scope,
        PolicyCondition condition,
        PolicyMetadata metadata,
        EffectiveDateRange effectiveDateRange,
        DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(policyType);
        ArgumentNullException.ThrowIfNull(policyCategory);
        ArgumentNullException.ThrowIfNull(policyReference);
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(effectiveDateRange);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Policy creation timestamp must be UTC.");
        }

        var policy = new Policy(id, policyType, policyCategory, policyReference, scope, createdAtUtc);

        var initialVersion = PolicyVersion.Create(1, effectiveDateRange, condition, metadata, createdAtUtc);
        policy._versions.Add(initialVersion);
        policy._snapshots.Add(PolicySnapshot.Capture(initialVersion, createdAtUtc));

        policy.Raise(new PolicyCreatedDomainEvent(
            policy.Id,
            policy.PolicyType.Value,
            policy.PolicyCategory.Value,
            policy.Scope.Kind.Value,
            policy.Scope.ScopeKey,
            createdAtUtc));

        return policy;
    }

    public PolicyVersion CreateVersion(
        PolicyCondition condition,
        PolicyMetadata metadata,
        EffectiveDateRange effectiveDateRange,
        DateTime createdAtUtc)
    {
        EnsureNotArchived();

        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(effectiveDateRange);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Policy version creation timestamp must be UTC.");
        }

        var nextVersionNumber = _versions.Count == 0 ? 1 : _versions.Max(x => x.VersionNumber) + 1;

        var nextVersion = PolicyVersion.Create(
            nextVersionNumber,
            effectiveDateRange,
            condition,
            metadata,
            createdAtUtc);

        _versions.Add(nextVersion);
        _snapshots.Add(PolicySnapshot.Capture(nextVersion, createdAtUtc));

        Raise(new PolicyVersionCreatedDomainEvent(Id, nextVersionNumber, createdAtUtc));

        return nextVersion;
    }

    public void ActivateVersion(int versionNumber, DateTime activatedAtUtc)
    {
        EnsureNotArchived();

        if (activatedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Policy activation timestamp must be UTC.");
        }

        var targetIndex = _versions.FindIndex(x => x.VersionNumber == versionNumber);
        if (targetIndex < 0)
        {
            throw new InvalidOperationException($"Policy version '{versionNumber}' was not found.");
        }

        var activeIndex = _versions.FindIndex(x => x.Status == PolicyStatus.Active);
        if (activeIndex >= 0 && activeIndex != targetIndex)
        {
            _versions[activeIndex] = _versions[activeIndex].Expire(activatedAtUtc);
        }

        _versions[targetIndex] = _versions[targetIndex].Activate(activatedAtUtc);

        Status = PolicyStatus.Active;
        ActivatedAtUtc = activatedAtUtc;
        ExpiredAtUtc = null;

        Raise(new PolicyActivatedDomainEvent(Id, versionNumber, activatedAtUtc));
    }

    public void Expire(DateTime expiredAtUtc)
    {
        EnsureNotArchived();

        if (expiredAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Policy expiration timestamp must be UTC.");
        }

        var activeIndex = _versions.FindIndex(x => x.Status == PolicyStatus.Active);
        if (activeIndex < 0)
        {
            throw new InvalidOperationException("No active policy version exists to expire.");
        }

        _versions[activeIndex] = _versions[activeIndex].Expire(expiredAtUtc);
        Status = PolicyStatus.Expired;
        ExpiredAtUtc = expiredAtUtc;

        Raise(new PolicyExpiredDomainEvent(Id, _versions[activeIndex].VersionNumber, expiredAtUtc));
    }

    public void Archive(DateTime archivedAtUtc, string reason)
    {
        if (archivedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Policy archive timestamp must be UTC.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        for (var i = 0; i < _versions.Count; i++)
        {
            if (_versions[i].Status != PolicyStatus.Archived)
            {
                _versions[i] = _versions[i].Archive(archivedAtUtc);
            }
        }

        Status = PolicyStatus.Archived;
        ArchivedAtUtc = archivedAtUtc;
        ArchivedReason = reason.Trim();

        Raise(new PolicyArchivedDomainEvent(Id, ArchivedReason, archivedAtUtc));
    }

    public void Assign(PolicyAssignment assignment)
    {
        EnsureNotArchived();

        ArgumentNullException.ThrowIfNull(assignment);

        var overlaps = _assignments.Any(existing =>
            string.Equals(existing.AssignedEntityType, assignment.AssignedEntityType, StringComparison.OrdinalIgnoreCase)
            && string.Equals(existing.AssignedEntityId, assignment.AssignedEntityId, StringComparison.OrdinalIgnoreCase)
            && existing.Scope == assignment.Scope
            && existing.EffectiveDateRange.Overlaps(assignment.EffectiveDateRange));

        if (overlaps)
        {
            throw new InvalidOperationException("Policy assignment overlaps an existing assignment for the same scope and entity.");
        }

        _assignments.Add(assignment);
    }

    public PolicyVersion? ResolveApplicableVersion(PolicyScope requestedScope, DateOnly asOfDate)
    {
        ArgumentNullException.ThrowIfNull(requestedScope);

        if (!Scope.AppliesTo(requestedScope))
        {
            return null;
        }

        var hasAssignments = _assignments.Count > 0;
        if (hasAssignments && !_assignments.Any(x => x.AppliesTo(requestedScope, asOfDate)))
        {
            return null;
        }

        return _versions
            .Where(x => x.Status == PolicyStatus.Active && x.EffectiveDateRange.Contains(asOfDate))
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefault();
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void EnsureNotArchived()
    {
        if (Status == PolicyStatus.Archived)
        {
            throw new InvalidOperationException("Archived policies are immutable.");
        }
    }

    private void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
