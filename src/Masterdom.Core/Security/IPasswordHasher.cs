namespace Masterdom.Core.Security;

/// <summary>
/// Hashes and verifies passwords. Contains no dependency on any specific
/// hashing framework or library -- concrete implementations adapt to this
/// contract, not the reverse.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Produces a hash for the given plaintext password. The plaintext is
    /// never returned or persisted.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verifies a plaintext password against a previously produced hash.
    /// </summary>
    bool Verify(string passwordHash, string password);
}
