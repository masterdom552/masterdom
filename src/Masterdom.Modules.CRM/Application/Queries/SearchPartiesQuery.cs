using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Queries;

/// <summary>
/// Query entry point for party search.
/// </summary>
public sealed record SearchPartiesQuery(string? DisplayNameContains, PartyType? PartyType, int Take = 50);
