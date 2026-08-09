using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Commands;

/// <summary>
/// Command entry point for removing an address.
/// </summary>
public sealed record RemoveAddressCommand(
    PartyId PartyId,
    Address Address,
    DateTime UpdatedAtUtc,
    string? UpdatedBy = null);
