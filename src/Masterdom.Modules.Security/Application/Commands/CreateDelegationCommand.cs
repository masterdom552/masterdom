namespace Masterdom.Modules.Security.Application.Commands;

/// <summary>
/// Command to create a new delegation of authority from the authenticated user to a delegatee.
///
/// The delegator is always the authenticated user - clients must not supply DelegatorUserId.
/// </summary>
public sealed record CreateDelegationCommand(
    Guid DelegateeUserId,
    Guid DelegatedRoleId,
    Guid[] PropertyIds,
    DateTime? EffectiveFromUtc,
    DateTime? EffectiveToUtc,
    string? Description,
    string? Remarks);
