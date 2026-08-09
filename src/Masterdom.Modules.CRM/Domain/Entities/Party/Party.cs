using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;
using Masterdom.Core.Primitives;
using Masterdom.Modules.CRM.Domain.Entities.Party.Events;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Represents a person or organization participating in Masterdom relationships.
/// </summary>
public sealed class Party : AggregateRoot<PartyId>, IHasDomainEvents
{
    private readonly List<ContactMethod> _contactMethods = [];
    private readonly List<Address> _addresses = [];
    private readonly List<Relationship> _relationships = [];
    private readonly List<PartyRoleAssignment> _roleAssignments = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    private Party(
        PartyId id,
        string displayName,
        string? legalName,
        PartyType partyType,
        PartyStatus status,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id)
    {
        DisplayName = NormalizeDisplayName(displayName);
        LegalName = NormalizeOptional(legalName);
        PartyType = partyType;
        Status = status;
        CreatedAtUtc = EnsureUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));
        AuditInfo = AuditInfo.Create(null);
    }

    public string DisplayName { get; private set; }

    public string? LegalName { get; private set; }

    public PartyType PartyType { get; private set; }

    public PartyStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }

    public AuditInfo AuditInfo { get; private set; }

    public IReadOnlyCollection<ContactMethod> ContactMethods => _contactMethods;

    public IReadOnlyCollection<Address> Addresses => _addresses;

    public IReadOnlyCollection<Relationship> Relationships => _relationships;

    public IReadOnlyCollection<PartyRoleAssignment> RoleAssignments => _roleAssignments;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Party Create(
        string displayName,
        string? legalName,
        PartyType partyType,
        DateTime createdAtUtc,
        string? createdBy = null)
    {
        ArgumentNullException.ThrowIfNull(partyType);

        var party = new Party(
            PartyId.New(),
            displayName,
            legalName,
            partyType,
            PartyStatus.Active,
            createdAtUtc,
            createdAtUtc);

        party.AuditInfo = AuditInfo.Create(createdBy);

        party.Raise(new PartyCreatedDomainEvent(party.Id, party.PartyType));

        return party;
    }

    public void Update(
        string displayName,
        string? legalName,
        PartyType partyType,
        DateTime updatedAtUtc,
        string? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(partyType);

        var normalizedDisplayName = NormalizeDisplayName(displayName);
        var normalizedLegalName = NormalizeOptional(legalName);

        if (DisplayName == normalizedDisplayName
            && LegalName == normalizedLegalName
            && PartyType == partyType)
        {
            return;
        }

        DisplayName = normalizedDisplayName;
        LegalName = normalizedLegalName;
        PartyType = partyType;
        Touch(updatedAtUtc, updatedBy);
        Raise(new PartyUpdatedDomainEvent(Id));
    }

    public void Deactivate(DateTime updatedAtUtc, string? updatedBy = null)
    {
        if (Status == PartyStatus.Inactive)
        {
            return;
        }

        Status = PartyStatus.Inactive;
        Touch(updatedAtUtc, updatedBy);
        Raise(new PartyDeactivatedDomainEvent(Id));
    }

    public void AddContactMethod(ContactMethod contactMethod, DateTime updatedAtUtc, string? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(contactMethod);

        var existingIndex = _contactMethods.FindIndex(x => x.Matches(contactMethod));
        if (existingIndex >= 0)
        {
            var existing = _contactMethods[existingIndex];
            if (contactMethod.IsPreferred && !existing.IsPreferred)
            {
                DemotePreferredContactMethods(contactMethod.Type);
                _contactMethods[existingIndex] = existing.WithPreferred(true);
                Touch(updatedAtUtc, updatedBy);
                Raise(new PartyUpdatedDomainEvent(Id));
            }

            return;
        }

        if (contactMethod.IsPreferred)
        {
            DemotePreferredContactMethods(contactMethod.Type);
        }

        _contactMethods.Add(contactMethod);
        Touch(updatedAtUtc, updatedBy);
        Raise(new PartyUpdatedDomainEvent(Id));
    }

    public bool RemoveContactMethod(ContactMethod contactMethod, DateTime updatedAtUtc, string? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(contactMethod);

        var removed = _contactMethods.RemoveAll(x => x.Matches(contactMethod)) > 0;
        if (!removed)
        {
            return false;
        }

        Touch(updatedAtUtc, updatedBy);
        Raise(new PartyUpdatedDomainEvent(Id));
        return true;
    }

    public void AddAddress(Address address, DateTime updatedAtUtc, string? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(address);

        var existingIndex = _addresses.FindIndex(x => x.Matches(address));
        if (existingIndex >= 0)
        {
            var existing = _addresses[existingIndex];
            if (address.IsPreferred && !existing.IsPreferred)
            {
                DemotePreferredAddresses();
                _addresses[existingIndex] = existing.WithPreferred(true);
                Touch(updatedAtUtc, updatedBy);
                Raise(new PartyUpdatedDomainEvent(Id));
            }

            return;
        }

        if (address.IsPreferred)
        {
            DemotePreferredAddresses();
        }

        _addresses.Add(address);
        Touch(updatedAtUtc, updatedBy);
        Raise(new PartyUpdatedDomainEvent(Id));
    }

    public bool RemoveAddress(Address address, DateTime updatedAtUtc, string? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(address);

        var removed = _addresses.RemoveAll(x => x.Matches(address)) > 0;
        if (!removed)
        {
            return false;
        }

        Touch(updatedAtUtc, updatedBy);
        Raise(new PartyUpdatedDomainEvent(Id));
        return true;
    }

    public void AddRelationship(Relationship relationship, DateTime updatedAtUtc, string? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        if (!relationship.AllowsSelfReference && relationship.RelatedPartyId == Id)
        {
            throw new InvalidOperationException("A relationship cannot reference the same party unless explicitly allowed.");
        }

        if (_relationships.Any(x => x.Matches(relationship)))
        {
            throw new InvalidOperationException(
                $"An active relationship of type '{relationship.Type.Value}' already exists for party '{relationship.RelatedPartyId}'.");
        }

        _relationships.Add(relationship);
        Touch(updatedAtUtc, updatedBy);
        Raise(new PartyUpdatedDomainEvent(Id));
    }

    public bool RemoveRelationship(Relationship relationship, DateTime updatedAtUtc, string? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        var removed = _relationships.RemoveAll(x => x.Matches(relationship)) > 0;
        if (!removed)
        {
            return false;
        }

        Touch(updatedAtUtc, updatedBy);
        Raise(new PartyUpdatedDomainEvent(Id));
        return true;
    }

    public PartyRoleAssignment AssignRole(
        PartyRoleType roleType,
        DateTime assignedAtUtc,
        DateTime? effectiveFromUtc = null,
        DateTime? effectiveToUtc = null,
        string? assignmentReason = null,
        string? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(roleType);

        var assignment = PartyRoleAssignment.Create(
            roleType,
            assignedAtUtc,
            effectiveFromUtc,
            effectiveToUtc,
            assignmentReason);

        if (_roleAssignments.Any(x =>
                x.RoleType == assignment.RoleType
                && x.Status == PartyRoleAssignmentStatus.Active
                && x.OverlapsWith(assignment.EffectiveFromUtc, assignment.EffectiveToUtc)))
        {
            throw new InvalidOperationException(
                $"An active role assignment for '{assignment.RoleType.Value}' already exists for the requested period.");
        }

        _roleAssignments.Add(assignment);
        Touch(assignedAtUtc, updatedBy);
        Raise(new PartyRoleAssignedDomainEvent(Id, assignment.Id, assignment.RoleType));

        return assignment;
    }

    public bool RemoveRole(PartyRoleAssignmentId roleAssignmentId, DateTime removedAtUtc, string? reason = null, string? updatedBy = null)
    {
        var assignment = GetRequiredRoleAssignment(roleAssignmentId);
        if (assignment is null)
        {
            return false;
        }

        assignment.Remove(removedAtUtc, reason);
        Touch(removedAtUtc, updatedBy);
        Raise(new PartyRoleRemovedDomainEvent(Id, assignment.Id, assignment.RoleType));

        return true;
    }

    public bool DeactivateRole(PartyRoleAssignmentId roleAssignmentId, DateTime deactivatedAtUtc, string? reason = null, string? updatedBy = null)
    {
        var assignment = GetRequiredRoleAssignment(roleAssignmentId);
        if (assignment is null)
        {
            return false;
        }

        assignment.Deactivate(deactivatedAtUtc, reason);
        Touch(deactivatedAtUtc, updatedBy);
        Raise(new PartyRoleDeactivatedDomainEvent(Id, assignment.Id, assignment.RoleType));

        return true;
    }

    public bool ReactivateRole(PartyRoleAssignmentId roleAssignmentId, DateTime reactivatedAtUtc, string? reason = null, string? updatedBy = null)
    {
        var assignment = GetRequiredRoleAssignment(roleAssignmentId);
        if (assignment is null)
        {
            return false;
        }

        if (_roleAssignments.Any(x =>
                x.Id != assignment.Id
                && x.RoleType == assignment.RoleType
                && x.Status == PartyRoleAssignmentStatus.Active
                && x.OverlapsWith(assignment.EffectiveFromUtc, assignment.EffectiveToUtc)))
        {
            throw new InvalidOperationException(
                $"An active role assignment for '{assignment.RoleType.Value}' already exists for the requested period.");
        }

        assignment.Reactivate(reactivatedAtUtc, reason);
        Touch(reactivatedAtUtc, updatedBy);
        Raise(new PartyRoleActivatedDomainEvent(Id, assignment.Id, assignment.RoleType));

        return true;
    }

    public IReadOnlyCollection<PartyRoleAssignment> GetRoles()
    {
        return _roleAssignments.AsReadOnly();
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void DemotePreferredContactMethods(ContactMethodType type)
    {
        for (var index = 0; index < _contactMethods.Count; index++)
        {
            var current = _contactMethods[index];
            if (current.Type == type && current.IsPreferred)
            {
                _contactMethods[index] = current.WithPreferred(false);
            }
        }
    }

    private void DemotePreferredAddresses()
    {
        for (var index = 0; index < _addresses.Count; index++)
        {
            var current = _addresses[index];
            if (current.IsPreferred)
            {
                _addresses[index] = current.WithPreferred(false);
            }
        }
    }

    private void Touch(DateTime updatedAtUtc, string? updatedBy)
    {
        UpdatedAtUtc = EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));
        AuditInfo = AuditInfo.WithUpdatedBy(updatedBy);
    }

    private PartyRoleAssignment? GetRequiredRoleAssignment(PartyRoleAssignmentId roleAssignmentId)
    {
        ArgumentNullException.ThrowIfNull(roleAssignmentId);
        return _roleAssignments.FirstOrDefault(x => x.Id == roleAssignmentId);
    }

    private void Raise(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    private static string NormalizeDisplayName(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        return displayName.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static DateTime EnsureUtc(DateTime value, string paramName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException($"{paramName} must be in UTC.");
        }

        return value;
    }
}
