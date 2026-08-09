using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Commands;

/// <summary>
/// Command entry point for deactivating a party role assignment.
/// </summary>
public sealed record DeactivatePartyRoleCommand(
    PartyId PartyId,
    PartyRoleAssignmentId RoleAssignmentId,
    DateTime DeactivatedAtUtc,
    string? Reason,
    string? UpdatedBy = null);
