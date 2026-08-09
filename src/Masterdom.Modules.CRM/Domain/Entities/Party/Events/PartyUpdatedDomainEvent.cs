using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.CRM.Domain.Entities.Party.Events;

/// <summary>
/// Domain fact emitted when a party changes.
/// </summary>
public sealed class PartyUpdatedDomainEvent : DomainEvent
{
    public PartyUpdatedDomainEvent(PartyId partyId)
    {
        ArgumentNullException.ThrowIfNull(partyId);
        PartyId = partyId;
    }

    public PartyId PartyId { get; }
}
