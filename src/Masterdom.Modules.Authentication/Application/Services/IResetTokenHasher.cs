namespace Masterdom.Modules.Authentication.Application.Services;

/// <summary>
/// Generates and verifies opaque, high-entropy password-reset tokens.
///
/// Deliberately distinct from <see cref="Masterdom.Core.Security.IPasswordHasher"/>:
/// that contract is tuned (adaptive, slow PBKDF2) for low-entropy,
/// human-memorable passwords, where slowness is the defense against brute
/// force. A reset token already carries sufficient entropy on its own; a
/// fast, standard hash is the correct primitive here, not a substitute
/// password hasher.
/// </summary>
public interface IResetTokenHasher
{
    /// <summary>
    /// Generates a new, cryptographically random, high-entropy token.
    /// </summary>
    string GenerateToken();

    /// <summary>
    /// Hashes a token for storage. The plaintext token is never persisted.
    /// </summary>
    string Hash(string token);

    /// <summary>
    /// Verifies a presented token against a previously produced hash.
    /// </summary>
    bool Verify(string tokenHash, string token);
}
