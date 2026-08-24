using Masterdom.Core.Security;
using Microsoft.AspNetCore.Identity;

namespace Masterdom.Modules.Authentication.Application.Services;

/// <summary>
/// Hashes and verifies passwords using the framework-provided adaptive
/// PBKDF2 algorithm (<see cref="PasswordHasher{TUser}"/>). No custom
/// cryptography is implemented here.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<object> _inner = new();

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return _inner.HashPassword(FrameworkUser, password);
    }

    public bool Verify(string passwordHash, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var result = _inner.VerifyHashedPassword(FrameworkUser, passwordHash, password);

        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }

    private static readonly object FrameworkUser = new();
}
