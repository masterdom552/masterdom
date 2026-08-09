using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Queries;

/// <summary>
/// Query entry point for party lookup by id.
/// </summary>
public sealed record GetPartyByIdQuery(PartyId PartyId);
