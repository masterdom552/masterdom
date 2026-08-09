using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Commands;

/// <summary>
/// Command entry point for adding a contact method.
/// </summary>
public sealed record AddContactMethodCommand(
    PartyId PartyId,
    ContactMethod ContactMethod,
    DateTime UpdatedAtUtc,
    string? UpdatedBy = null);
