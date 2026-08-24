namespace Masterdom.Modules.Authentication.Application.Commands;

/// <summary>
/// Anonymous password reset redemption. The username is included alongside
/// the opaque token so the pending reset can be located via the existing
/// user-indexed lookup.
/// </summary>
public sealed record CompletePasswordResetCommand(string Username, string Token, string NewPassword);
