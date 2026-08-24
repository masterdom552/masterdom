namespace Masterdom.Modules.Authentication.Application.Commands;

/// <summary>
/// Authenticated self-service password change. Carries no user identifier
/// -- the acting user is resolved exclusively from the server-side current
/// user context, never from client input.
/// </summary>
public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword);
