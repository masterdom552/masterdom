namespace Masterdom.Modules.Security.Application.Commands;

/// <summary>
/// Command to revoke a previously granted delegation of authority.
///
/// Only the delegator or a higher authority can revoke.
/// The acting user is always the authenticated user.
/// </summary>
public sealed record RevokeDelegationCommand(
    Guid DelegatedAuthorityId,
    string? RevocationReason);
