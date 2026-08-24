using Masterdom.Modules.Authentication.Application.Services;

namespace Masterdom.Core.Tests.Authentication;

public sealed class ResetTokenHasherTests
{
    [Fact]
    public void Verify_WithCorrectToken_ShouldReturnTrue()
    {
        var hasher = new ResetTokenHasher();
        var token = hasher.GenerateToken();
        var hash = hasher.Hash(token);

        Assert.True(hasher.Verify(hash, token));
    }

    [Fact]
    public void Verify_WithIncorrectToken_ShouldReturnFalse()
    {
        var hasher = new ResetTokenHasher();
        var hash = hasher.Hash(hasher.GenerateToken());

        Assert.False(hasher.Verify(hash, hasher.GenerateToken()));
    }

    [Fact]
    public void GenerateToken_CalledTwice_ShouldProduceDifferentTokens()
    {
        var hasher = new ResetTokenHasher();

        var tokenA = hasher.GenerateToken();
        var tokenB = hasher.GenerateToken();

        Assert.NotEqual(tokenA, tokenB);
    }

    [Fact]
    public void Hash_ShouldNotReturnThePlaintextToken()
    {
        var hasher = new ResetTokenHasher();
        var token = hasher.GenerateToken();

        var hash = hasher.Hash(token);

        Assert.NotEqual(token, hash);
    }

    [Fact]
    public void Hash_CalledTwiceForSameToken_ShouldProduceTheSameHash()
    {
        var hasher = new ResetTokenHasher();
        var token = hasher.GenerateToken();

        var hashA = hasher.Hash(token);
        var hashB = hasher.Hash(token);

        Assert.Equal(hashA, hashB);
    }
}
