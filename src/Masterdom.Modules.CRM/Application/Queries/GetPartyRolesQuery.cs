using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Queries;

/// <summary>
/// Query entry point for retrieving all historical roles for a party.
/// </summary>
public sealed record GetPartyRolesQuery(PartyId PartyId);
