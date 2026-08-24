using Masterdom.Core.Identity.Entities.Credential;
using Masterdom.Core.Identity.Entities.User;

namespace Masterdom.Core.Tests.Identity;

public sealed class CredentialTests
{
    [Fact]
    public void Create_ShouldInitializeActiveCredential()
    {
        var userId = UserId.New();

        var credential = Credential.Create(userId, "hashed-value");

        Assert.Equal(userId, credential.UserId);
        Assert.Equal("hashed-value", credential.PasswordHash);
        Assert.Equal(CredentialStatus.Active, credential.Status);
        Assert.Equal(credential.CreatedAtUtc, credential.ChangedAtUtc);
    }

    [Fact]
    public void Create_WithNullUserId_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(
            () => Credential.Create(null!, "hashed-value"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidPasswordHash_ShouldThrow(string? passwordHash)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => Credential.Create(UserId.New(), passwordHash!));
    }

    [Fact]
    public void ChangePassword_ShouldReplaceHashAndUpdateTimestamp()
    {
        var credential = Credential.Create(UserId.New(), "original-hash");
        var createdAtUtc = credential.CreatedAtUtc;

        credential.ChangePassword("new-hash");

        Assert.Equal("new-hash", credential.PasswordHash);
        Assert.Equal(createdAtUtc, credential.CreatedAtUtc);
        Assert.True(credential.ChangedAtUtc >= createdAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ChangePassword_WithInvalidHash_ShouldThrow(string? newPasswordHash)
    {
        var credential = Credential.Create(UserId.New(), "original-hash");

        Assert.ThrowsAny<ArgumentException>(
            () => credential.ChangePassword(newPasswordHash!));
    }

    [Fact]
    public void Revoke_ShouldSetStatusToRevoked()
    {
        var credential = Credential.Create(UserId.New(), "hashed-value");

        credential.Revoke();

        Assert.Equal(CredentialStatus.Revoked, credential.Status);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldThrow()
    {
        var credential = Credential.Create(UserId.New(), "hashed-value");
        credential.Revoke();

        Assert.Throws<InvalidOperationException>(() => credential.Revoke());
    }
}
