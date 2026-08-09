using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Commands;

/// <summary>
/// Command entry point for party creation.
/// </summary>
public sealed record CreatePartyCommand(
    string DisplayName,
    string? LegalName,
    PartyType PartyType,
    DateTime CreatedAtUtc,
    string? CreatedBy = null);
