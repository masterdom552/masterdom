using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Domain.Repositories;

/// <summary>
/// Provides aggregate persistence boundaries for CRM parties.
/// </summary>
public interface IPartyRepository
{
    Party? GetById(PartyId id);

    IReadOnlyCollection<Party> Search(string? displayNameContains, PartyType? partyType, int take);

    IReadOnlyCollection<Party> SearchByRole(PartyRoleType roleType, DateTime asOfUtc, int take);

    void Add(Party party);

    void Update(Party party);
}
