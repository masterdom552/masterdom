using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Commands;

/// <summary>
/// Command entry point for removing a party role assignment.
/// </summary>
public sealed record RemovePartyRoleCommand(
    PartyId PartyId,
    PartyRoleAssignmentId RoleAssignmentId,
    DateTime RemovedAtUtc,
    string? Reason,
    string? UpdatedBy = null);
