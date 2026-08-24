using Masterdom.Modules.Authentication.Application.Services;

namespace Masterdom.Core.Tests.Authentication;

public sealed class PasswordHasherTests
{
    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("correct-password-1");

        Assert.True(hasher.Verify(hash, "correct-password-1"));
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("correct-password-1");

        Assert.False(hasher.Verify(hash, "wrong-password"));
    }

    [Fact]
    public void Hash_ShouldNotReturnThePlaintextPassword()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("correct-password-1");

        Assert.NotEqual("correct-password-1", hash);
    }

    [Fact]
    public void Hash_CalledTwiceForSamePassword_ShouldProduceDifferentHashes()
    {
        var hasher = new PasswordHasher();

        var hash1 = hasher.Hash("correct-password-1");
        var hash2 = hasher.Hash("correct-password-1");

        Assert.NotEqual(hash1, hash2);
        Assert.True(hasher.Verify(hash1, "correct-password-1"));
        Assert.True(hasher.Verify(hash2, "correct-password-1"));
    }
}
