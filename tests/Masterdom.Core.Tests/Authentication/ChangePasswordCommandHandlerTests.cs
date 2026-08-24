using Masterdom.Core.Identity.Entities.Credential;
using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Security;
using Masterdom.Modules.Authentication.Application.Commands;
using Masterdom.Modules.Authentication.Application.Handlers;
using Masterdom.Modules.Authentication.Application.Services;
using Masterdom.Modules.Authentication.Application.Support;

namespace Masterdom.Core.Tests.Authentication;

public sealed class ChangePasswordCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithCorrectCurrentPassword_ShouldSucceedAndNewPasswordVerifies()
    {
        var passwordHasher = new PasswordHasher();
        var (handler, _, credential) = CreateHandler(passwordHasher, "old-password");

        var result = await handler.HandleAsync(new ChangePasswordCommand("old-password", "new-password-123"));

        Assert.True(result.IsSuccess);
        Assert.True(passwordHasher.Verify(credential.PasswordHash, "new-password-123"));
        Assert.False(passwordHasher.Verify(credential.PasswordHash, "old-password"));
    }

    [Fact]
    public async Task HandleAsync_WithWrongCurrentPassword_ShouldFailAndLeaveCredentialUnchanged()
    {
        var passwordHasher = new PasswordHasher();
        var (handler, _, credential) = CreateHandler(passwordHasher, "old-password");
        var originalHash = credential.PasswordHash;

        var result = await handler.HandleAsync(new ChangePasswordCommand("wrong-password", "new-password-123"));

        Assert.False(result.IsSuccess);
        Assert.Equal("unauthorized", result.ErrorCode);
        Assert.Equal(originalHash, credential.PasswordHash);
    }

    [Fact]
    public async Task HandleAsync_WithUnauthenticatedCaller_ShouldFail()
    {
        var passwordHasher = new PasswordHasher();
        var user = CreateUser();
        var credential = Credential.Create(user.Id, passwordHasher.Hash("old-password"));

        var handler = new ChangePasswordCommandHandler(
            new FakeCurrentUserAccessor(CurrentUser.Anonymous),
            new FakeCredentialRepository(credential),
            passwordHasher,
            new FakeAuthenticationUnitOfWork());

        var result = await handler.HandleAsync(new ChangePasswordCommand("old-password", "new-password-123"));

        Assert.False(result.IsSuccess);
        Assert.Equal("unauthorized", result.ErrorCode);
    }

    [Fact]
    public async Task HandleAsync_WithNewPasswordTooShort_ShouldFailValidationAndLeaveCredentialUnchanged()
    {
        var passwordHasher = new PasswordHasher();
        var (handler, _, credential) = CreateHandler(passwordHasher, "old-password");
        var originalHash = credential.PasswordHash;

        var result = await handler.HandleAsync(new ChangePasswordCommand("old-password", "short"));

        Assert.False(result.IsSuccess);
        Assert.Equal("validation_failed", result.ErrorCode);
        Assert.Equal(originalHash, credential.PasswordHash);
    }

    [Fact]
    public async Task HandleAsync_FailureResult_ShouldNeverExposePasswordOrHash()
    {
        var passwordHasher = new PasswordHasher();
        var (handler, _, credential) = CreateHandler(passwordHasher, "old-password");

        var result = await handler.HandleAsync(new ChangePasswordCommand("wrong-password", "new-password-123"));

        Assert.DoesNotContain("old-password", result.ErrorMessage);
        Assert.DoesNotContain("new-password-123", result.ErrorMessage);
        Assert.DoesNotContain(credential.PasswordHash, result.ErrorMessage ?? string.Empty);
    }

    private static (ChangePasswordCommandHandler Handler, User User, Credential Credential) CreateHandler(
        IPasswordHasher passwordHasher,
        string currentPassword)
    {
        var user = CreateUser();
        var credential = Credential.Create(user.Id, passwordHasher.Hash(currentPassword));

        var currentUser = CurrentUser.Authenticated(
            user.Id.Value,
            personId: null,
            user.Username.Value,
            roles: [],
            permissions: [],
            propertyScopes: [],
            ownedPropertyIds: []);

        var handler = new ChangePasswordCommandHandler(
            new FakeCurrentUserAccessor(currentUser),
            new FakeCredentialRepository(credential),
            passwordHasher,
            new FakeAuthenticationUnitOfWork());

        return (handler, user, credential);
    }

    private static User CreateUser()
    {
        return User.Create(
            UserCode.Create("USR-CHANGEPWD-001"),
            IdentityProfileId.New(),
            Username.Create("change-password-test-user"));
    }

    private sealed class FakeCurrentUserAccessor : ICurrentUserAccessor
    {
        private readonly CurrentUser _currentUser;

        public FakeCurrentUserAccessor(CurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public CurrentUser GetCurrentUser() => _currentUser;
    }

    private sealed class FakeCredentialRepository : ICredentialRepository
    {
        private readonly Credential _credential;

        public FakeCredentialRepository(Credential credential)
        {
            _credential = credential;
        }

        public Task<Credential?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_credential.UserId == userId ? _credential : null);
        }

        public void Add(Credential credential)
        {
        }
    }

    private sealed class FakeAuthenticationUnitOfWork : IAuthenticationUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
