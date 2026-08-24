namespace Masterdom.Modules.Authentication.Application.Models;

/// <summary>
/// Carries the plaintext reset secret exactly once, at creation time. It is
/// never persisted -- only its hash is stored -- and never logged.
/// </summary>
public sealed record RequestPasswordResetResult(string ResetToken, DateTime ExpiresAtUtc);
