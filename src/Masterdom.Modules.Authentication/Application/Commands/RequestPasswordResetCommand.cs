namespace Masterdom.Modules.Authentication.Application.Commands;

/// <summary>
/// Administrator-mediated password reset request. The acting administrator
/// is resolved from the server-side current-user context; only the target
/// username is client-supplied.
/// </summary>
public sealed record RequestPasswordResetCommand(string TargetUsername);
