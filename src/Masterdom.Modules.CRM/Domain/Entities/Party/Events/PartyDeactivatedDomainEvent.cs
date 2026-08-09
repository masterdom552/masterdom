using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.CRM.Domain.Entities.Party.Events;

/// <summary>
/// Domain fact emitted when a party is deactivated.
/// </summary>
public sealed class PartyDeactivatedDomainEvent : DomainEvent
{
    public PartyDeactivatedDomainEvent(PartyId partyId)
    {
        ArgumentNullException.ThrowIfNull(partyId);
        PartyId = partyId;
    }

    public PartyId PartyId { get; }
}
