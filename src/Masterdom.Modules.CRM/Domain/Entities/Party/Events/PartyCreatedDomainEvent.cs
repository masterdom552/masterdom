using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.CRM.Domain.Entities.Party.Events;

/// <summary>
/// Domain fact emitted when a party is created.
/// </summary>
public sealed class PartyCreatedDomainEvent : DomainEvent
{
    public PartyCreatedDomainEvent(PartyId partyId, PartyType partyType)
    {
        ArgumentNullException.ThrowIfNull(partyId);
        ArgumentNullException.ThrowIfNull(partyType);

        PartyId = partyId;
        PartyType = partyType;
    }

    public PartyId PartyId { get; }

    public PartyType PartyType { get; }
}
