using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Commands;

/// <summary>
/// Command entry point for party deactivation.
/// </summary>
public sealed record DeactivatePartyCommand(
    PartyId PartyId,
    DateTime UpdatedAtUtc,
    string? UpdatedBy = null);
