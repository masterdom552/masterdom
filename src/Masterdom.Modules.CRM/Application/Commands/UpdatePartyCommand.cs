using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Commands;

/// <summary>
/// Command entry point for party updates.
/// </summary>
public sealed record UpdatePartyCommand(
    PartyId PartyId,
    string DisplayName,
    string? LegalName,
    PartyType PartyType,
    DateTime UpdatedAtUtc,
    string? UpdatedBy = null);
