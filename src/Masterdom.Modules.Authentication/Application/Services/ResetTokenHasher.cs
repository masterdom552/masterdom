using System.Security.Cryptography;
using System.Text;

namespace Masterdom.Modules.Authentication.Application.Services;

/// <summary>
/// Framework-standard implementation of <see cref="IResetTokenHasher"/>:
/// a cryptographically random 256-bit token, hashed for storage with
/// SHA-256 -- a standard .NET primitive, not invented cryptography.
/// </summary>
public sealed class ResetTokenHasher : IResetTokenHasher
{
    private const int TokenSizeInBytes = 32;

    public string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenSizeInBytes);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(bytes);
    }

    public bool Verify(string tokenHash, string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var computedHash = Hash(token);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(tokenHash));
    }
}
