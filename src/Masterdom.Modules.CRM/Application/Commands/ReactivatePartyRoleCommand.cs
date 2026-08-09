using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Commands;

/// <summary>
/// Command entry point for reactivating a party role assignment.
/// </summary>
public sealed record ReactivatePartyRoleCommand(
    PartyId PartyId,
    PartyRoleAssignmentId RoleAssignmentId,
    DateTime ReactivatedAtUtc,
    string? Reason,
    string? UpdatedBy = null);
