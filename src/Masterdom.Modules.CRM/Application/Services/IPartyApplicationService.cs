using Masterdom.Modules.CRM.Application.Commands;
using Masterdom.Modules.CRM.Application.Queries;
using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Services;

/// <summary>
/// Orchestrates CRM party use-cases through aggregate APIs.
/// </summary>
public interface IPartyApplicationService
{
    Party CreateParty(CreatePartyCommand command);

    Party UpdateParty(UpdatePartyCommand command);

    Party DeactivateParty(DeactivatePartyCommand command);

    Party AddContactMethod(AddContactMethodCommand command);

    bool RemoveContactMethod(RemoveContactMethodCommand command);

    Party AddAddress(AddAddressCommand command);

    bool RemoveAddress(RemoveAddressCommand command);

    Party CreateRelationship(CreateRelationshipCommand command);

    bool RemoveRelationship(RemoveRelationshipCommand command);

    Party AssignPartyRole(AssignPartyRoleCommand command);

    bool RemovePartyRole(RemovePartyRoleCommand command);

    bool DeactivatePartyRole(DeactivatePartyRoleCommand command);

    bool ReactivatePartyRole(ReactivatePartyRoleCommand command);

    Party? GetParty(GetPartyByIdQuery query);

    IReadOnlyCollection<PartyRoleAssignment> GetPartyRoles(GetPartyRolesQuery query);

    IReadOnlyCollection<Party> SearchParties(SearchPartiesQuery query);

    IReadOnlyCollection<Party> SearchPartiesByRole(SearchPartiesByRoleQuery query);
}
