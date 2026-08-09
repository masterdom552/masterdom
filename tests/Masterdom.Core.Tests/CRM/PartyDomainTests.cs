using Masterdom.Modules.CRM.Domain.Entities.Party;
using Masterdom.Modules.CRM.Domain.Entities.Party.Events;

namespace Masterdom.Core.Tests.CRM;

public sealed class PartyDomainTests
{
    [Fact]
    public void Create_ShouldInitializePartyAndRaiseCreatedEvent()
    {
        var createdAtUtc = DateTime.UtcNow;

        var party = Party.Create(
            "Apex Property Services",
            "Apex Property Services LLC",
            PartyType.Organization,
            createdAtUtc,
            createdBy: "system");

        Assert.Equal("Apex Property Services", party.DisplayName);
        Assert.Equal("Apex Property Services LLC", party.LegalName);
        Assert.Equal(PartyType.Organization, party.PartyType);
        Assert.Equal(PartyStatus.Active, party.Status);
        Assert.Equal(createdAtUtc, party.CreatedAtUtc);
        Assert.Contains(party.DomainEvents, x => x is PartyCreatedDomainEvent);
    }

    [Fact]
    public void Create_ShouldRejectBlankDisplayName()
    {
        var createdAtUtc = DateTime.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            Party.Create(" ", null, PartyType.Person, createdAtUtc));
    }

    [Fact]
    public void AddAddress_ShouldKeepOnlyOnePreferredAddress()
    {
        var party = CreateParty();

        party.AddAddress(
            Address.Create("Business", "1 Main Street", null, "Lagos", "Lagos", "100001", "Nigeria", isPreferred: true),
            DateTime.UtcNow);

        party.AddAddress(
            Address.Create("Billing", "2 Broad Street", null, "Lagos", "Lagos", "100002", "Nigeria", isPreferred: true),
            DateTime.UtcNow);

        Assert.Equal(2, party.Addresses.Count);
        Assert.Single(party.Addresses, x => x.IsPreferred);
        Assert.Equal("2 Broad Street", party.Addresses.Single(x => x.IsPreferred).Line1);
    }

    [Fact]
    public void AddContactMethod_ShouldKeepOnlyOnePreferredMethodPerType()
    {
        var party = CreateParty();

        party.AddContactMethod(ContactMethod.Create("Email", "primary@example.com", isPreferred: true), DateTime.UtcNow);
        party.AddContactMethod(ContactMethod.Create("Email", "secondary@example.com", isPreferred: true), DateTime.UtcNow);
        party.AddContactMethod(ContactMethod.Create("Phone", "+2348000000001", isPreferred: true), DateTime.UtcNow);

        Assert.Equal(3, party.ContactMethods.Count);
        Assert.Single(party.ContactMethods, x => x.Type == ContactMethodType.Email && x.IsPreferred);
        Assert.Single(party.ContactMethods, x => x.Type == ContactMethodType.Phone && x.IsPreferred);
        Assert.Equal("secondary@example.com", party.ContactMethods.Single(x => x.Type == ContactMethodType.Email && x.IsPreferred).Value);
    }

    [Fact]
    public void AddRelationship_ShouldRejectSelfReferenceWhenNotExplicitlyAllowed()
    {
        var party = CreateParty();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            party.AddRelationship(
                Relationship.Create(party.Id, RelationshipType.EmergencyContact),
                DateTime.UtcNow));

        Assert.Contains("same party", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddRelationship_ShouldPreventDuplicateActiveRelationship()
    {
        var party = CreateParty();
        var relatedPartyId = PartyId.New();

        party.AddRelationship(Relationship.Create(relatedPartyId, RelationshipType.SupplierOf), DateTime.UtcNow);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            party.AddRelationship(Relationship.Create(relatedPartyId, RelationshipType.SupplierOf), DateTime.UtcNow));

        Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AssignRole_ShouldAllowMultipleDistinctRoles()
    {
        var party = CreateParty();

        party.AssignRole(PartyRoleType.Tenant, DateTime.UtcNow, assignmentReason: "Primary occupancy");
        party.AssignRole(PartyRoleType.Vendor, DateTime.UtcNow, assignmentReason: "Preferred supplier");

        Assert.Equal(2, party.RoleAssignments.Count);
        Assert.Contains(party.RoleAssignments, x => x.RoleType == PartyRoleType.Tenant);
        Assert.Contains(party.RoleAssignments, x => x.RoleType == PartyRoleType.Vendor);
    }

    [Fact]
    public void AssignRole_ShouldPreventDuplicateActiveAssignments()
    {
        var party = CreateParty();
        var assignedAtUtc = DateTime.UtcNow;

        party.AssignRole(PartyRoleType.PropertyOwner, assignedAtUtc, assignmentReason: "Initial assignment");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            party.AssignRole(PartyRoleType.PropertyOwner, assignedAtUtc.AddMinutes(1), assignmentReason: "Duplicate assignment"));

        Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DeactivateRole_ShouldPreserveHistoricalAssignment()
    {
        var party = CreateParty();
        var assignment = party.AssignRole(PartyRoleType.Contractor, DateTime.UtcNow, assignmentReason: "Maintenance support");

        var deactivated = party.DeactivateRole(assignment.Id, DateTime.UtcNow.AddHours(1), "Contract complete");

        Assert.True(deactivated);
        Assert.Single(party.RoleAssignments);
        Assert.Equal(PartyRoleAssignmentStatus.Inactive, assignment.Status);
        Assert.NotNull(assignment.DeactivatedAtUtc);
    }

    [Fact]
    public void ReactivateRole_ShouldRejectExpiredAssignment()
    {
        var party = CreateParty();
        var assignedAtUtc = DateTime.UtcNow;

        var assignment = party.AssignRole(
            PartyRoleType.Employee,
            assignedAtUtc,
            effectiveToUtc: assignedAtUtc.AddDays(1),
            assignmentReason: "Seasonal staffing");

        party.DeactivateRole(assignment.Id, assignedAtUtc.AddHours(2), "End of shift");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            party.ReactivateRole(assignment.Id, assignedAtUtc.AddDays(2), "Resume work"));

        Assert.Contains("expired", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RemoveRole_ShouldRetainRoleHistory()
    {
        var party = CreateParty();
        var assignment = party.AssignRole(PartyRoleType.Broker, DateTime.UtcNow, assignmentReason: "Listing support");

        var removed = party.RemoveRole(assignment.Id, DateTime.UtcNow.AddMinutes(30), "No longer engaged");

        Assert.True(removed);
        Assert.Single(party.RoleAssignments);
        Assert.Equal(PartyRoleAssignmentStatus.Removed, assignment.Status);
        Assert.NotNull(assignment.RemovedAtUtc);
    }

    [Fact]
    public void RoleAssignment_IsEffectiveAt_ShouldRespectEffectiveWindow()
    {
        var assignedAtUtc = DateTime.UtcNow;
        var effectiveFromUtc = assignedAtUtc.AddDays(1);
        var effectiveToUtc = assignedAtUtc.AddDays(5);

        var assignment = PartyRoleAssignment.Create(
            PartyRoleType.UtilityProvider,
            assignedAtUtc,
            effectiveFromUtc,
            effectiveToUtc,
            assignmentReason: "Utility contract");

        Assert.False(assignment.IsEffectiveAt(assignedAtUtc));
        Assert.True(assignment.IsEffectiveAt(effectiveFromUtc.AddHours(1)));
        Assert.False(assignment.IsEffectiveAt(effectiveToUtc.AddHours(1)));
    }

    private static Party CreateParty()
    {
        return Party.Create(
            "Sample Party",
            null,
            PartyType.Organization,
            DateTime.UtcNow,
            createdBy: "tester");
    }
}
