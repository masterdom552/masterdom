using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Commands;

/// <summary>
/// Command entry point for assigning a role to a party.
/// </summary>
public sealed record AssignPartyRoleCommand(
    PartyId PartyId,
    PartyRoleType RoleType,
    DateTime AssignedAtUtc,
    DateTime? EffectiveFromUtc,
    DateTime? EffectiveToUtc,
    string? AssignmentReason,
    string? UpdatedBy = null);
