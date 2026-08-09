using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.CRM.Domain.Entities.Party.Events;

/// <summary>
/// Domain fact emitted when a party role becomes inactive.
/// </summary>
public sealed class PartyRoleDeactivatedDomainEvent : DomainEvent
{
    public PartyRoleDeactivatedDomainEvent(PartyId partyId, PartyRoleAssignmentId roleAssignmentId, PartyRoleType roleType)
    {
        ArgumentNullException.ThrowIfNull(partyId);
        ArgumentNullException.ThrowIfNull(roleAssignmentId);
        ArgumentNullException.ThrowIfNull(roleType);

        PartyId = partyId;
        RoleAssignmentId = roleAssignmentId;
        RoleType = roleType;
    }

    public PartyId PartyId { get; }

    public PartyRoleAssignmentId RoleAssignmentId { get; }

    public PartyRoleType RoleType { get; }
}
