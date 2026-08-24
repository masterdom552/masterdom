using Masterdom.Core.Identity.Entities.Credential;
using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Core.Identity.Entities.PasswordReset;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Security;
using Masterdom.Modules.Authentication.Application.Commands;
using Masterdom.Modules.Authentication.Application.Handlers;
using Masterdom.Modules.Authentication.Application.Services;
using Masterdom.Modules.Authentication.Application.Support;

namespace Masterdom.Core.Tests.Authentication;

public sealed class CompletePasswordResetCommandHandlerTests
{
    private const string GenericErrorCode = "unauthorized";

    [Fact]
    public async Task HandleAsync_WithValidUsernameTokenAndPassword_ShouldSucceedAndReplacePassword()
    {
        var passwordHasher = new PasswordHasher();
        var resetTokenHasher = new ResetTokenHasher();
        var user = CreateUser();
        var credential = Credential.Create(user.Id, passwordHasher.Hash("old-password"));
        var token = resetTokenHasher.GenerateToken();
        var reset = PasswordReset.Create(user.Id, resetTokenHasher.Hash(token), TimeSpan.FromMinutes(15));

        var handler = CreateHandler(passwordHasher, resetTokenHasher, user, credential, reset);

        var result = await handler.HandleAsync(
            new CompletePasswordResetCommand(user.Username.Value, token, "brand-new-password"));

        Assert.True(result.IsSuccess);
        Assert.True(passwordHasher.Verify(credential.PasswordHash, "brand-new-password"));
        Assert.False(passwordHasher.Verify(credential.PasswordHash, "old-password"));
        Assert.Equal(PasswordResetStatus.Completed, reset.Status);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownUsername_ShouldFailGenerically()
    {
        var passwordHasher = new PasswordHasher();
        var resetTokenHasher = new ResetTokenHasher();
        var user = CreateUser();
        var credential = Credential.Create(user.Id, passwordHasher.Hash("old-password"));
        var token = resetTokenHasher.GenerateToken();
        var reset = PasswordReset.Create(user.Id, resetTokenHasher.Hash(token), TimeSpan.FromMinutes(15));

        var handler = CreateHandler(passwordHasher, resetTokenHasher, user, credential, reset);

        var result = await handler.HandleAsync(
            new CompletePasswordResetCommand("nonexistent-user", token, "brand-new-password"));

        AssertGenericFailure(result);
    }

    [Fact]
    public async Task HandleAsync_WithNoPendingReset_ShouldFailGenerically()
    {
        var passwordHasher = new PasswordHasher();
        var resetTokenHasher = new ResetTokenHasher();
        var user = CreateUser();
        var credential = Credential.Create(user.Id, passwordHasher.Hash("old-password"));

        var handler = CreateHandler(passwordHasher, resetTokenHasher, user, credential, existingReset: null);

        var result = await handler.HandleAsync(
            new CompletePasswordResetCommand(user.Username.Value, "some-token", "brand-new-password"));

        AssertGenericFailure(result);
    }

    [Fact]
    public async Task HandleAsync_WithExpiredReset_ShouldFailGenerically()
    {
        var passwordHasher = new PasswordHasher();
        var resetTokenHasher = new ResetTokenHasher();
        var user = CreateUser();
        var credential = Credential.Create(user.Id, passwordHasher.Hash("old-password"));
        var token = resetTokenHasher.GenerateToken();
        var reset = PasswordReset.Create(user.Id, resetTokenHasher.Hash(token), TimeSpan.FromMilliseconds(1));

        await Task.Delay(20);

        var handler = CreateHandler(passwordHasher, resetTokenHasher, user, credential, reset);

        var result = await handler.HandleAsync(
            new CompletePasswordResetCommand(user.Username.Value, token, "brand-new-password"));

        AssertGenericFailure(result);
    }

    [Fact]
    public async Task HandleAsync_WithWrongToken_ShouldFailGenerically()
    {
        var passwordHasher = new PasswordHasher();
        var resetTokenHasher = new ResetTokenHasher();
        var user = CreateUser();
        var credential = Credential.Create(user.Id, passwordHasher.Hash("old-password"));
        var token = resetTokenHasher.GenerateToken();
        var reset = PasswordReset.Create(user.Id, resetTokenHasher.Hash(token), TimeSpan.FromMinutes(15));

        var handler = CreateHandler(passwordHasher, resetTokenHasher, user, credential, reset);

        var result = await handler.HandleAsync(
            new CompletePasswordResetCommand(user.Username.Value, "wrong-token", "brand-new-password"));

        AssertGenericFailure(result);
    }

    [Fact]
    public async Task HandleAsync_WithWeakNewPassword_ShouldFailValidationAndNotConsumeToken()
    {
        var passwordHasher = new PasswordHasher();
        var resetTokenHasher = new ResetTokenHasher();
        var user = CreateUser();
        var credential = Credential.Create(user.Id, passwordHasher.Hash("old-password"));
        var token = resetTokenHasher.GenerateToken();
        var reset = PasswordReset.Create(user.Id, resetTokenHasher.Hash(token), TimeSpan.FromMinutes(15));

        var handler = CreateHandler(passwordHasher, resetTokenHasher, user, credential, reset);

        var result = await handler.HandleAsync(
            new CompletePasswordResetCommand(user.Username.Value, token, "short"));

        Assert.False(result.IsSuccess);
        Assert.Equal("validation_failed", result.ErrorCode);
        Assert.Equal(PasswordResetStatus.Pending, reset.Status);
    }

    [Fact]
    public async Task HandleAsync_RedeemedTwiceWithSameToken_ShouldSucceedOnceAndFailGenericallyOnSecondAttempt()
    {
        var passwordHasher = new PasswordHasher();
        var resetTokenHasher = new ResetTokenHasher();
        var user = CreateUser();
        var credential = Credential.Create(user.Id, passwordHasher.Hash("old-password"));
        var token = resetTokenHasher.GenerateToken();
        var reset = PasswordReset.Create(user.Id, resetTokenHasher.Hash(token), TimeSpan.FromMinutes(15));

        var handler = CreateHandler(passwordHasher, resetTokenHasher, user, credential, reset);

        var first = await handler.HandleAsync(
            new CompletePasswordResetCommand(user.Username.Value, token, "brand-new-password"));
        var second = await handler.HandleAsync(
            new CompletePasswordResetCommand(user.Username.Value, token, "another-new-password"));

        Assert.True(first.IsSuccess);
        AssertGenericFailure(second);
        Assert.True(passwordHasher.Verify(credential.PasswordHash, "brand-new-password"));
    }

    [Fact]
    public async Task HandleAsync_TwoConcurrentRedemptions_ShouldResultInExactlyOneSuccess()
    {
        var passwordHasher = new PasswordHasher();
        var resetTokenHasher = new ResetTokenHasher();
        var user = CreateUser();
        var credential = Credential.Create(user.Id, passwordHasher.Hash("old-password"));
        var token = resetTokenHasher.GenerateToken();
        var reset = PasswordReset.Create(user.Id, resetTokenHasher.Hash(token), TimeSpan.FromMinutes(15));

        var userRepository = new FakeUserRepository(user);
        var passwordResetRepository = new FakePasswordResetRepository(reset);
        var credentialRepository = new FakeCredentialRepository(credential);

        var handlerA = new CompletePasswordResetCommandHandler(
            userRepository, passwordResetRepository, resetTokenHasher, credentialRepository, passwordHasher,
            new FakeAuthenticationUnitOfWork());
        var handlerB = new CompletePasswordResetCommandHandler(
            userRepository, passwordResetRepository, resetTokenHasher, credentialRepository, passwordHasher,
            new FakeAuthenticationUnitOfWork());

        var resultsTask = Task.WhenAll(
            handlerA.HandleAsync(new CompletePasswordResetCommand(user.Username.Value, token, "password-from-a")),
            handlerB.HandleAsync(new CompletePasswordResetCommand(user.Username.Value, token, "password-from-b")));

        var results = await resultsTask;

        Assert.Single(results, r => r.IsSuccess);
        Assert.Single(results, r => !r.IsSuccess);
    }

    private static CompletePasswordResetCommandHandler CreateHandler(
        IPasswordHasher passwordHasher,
        IResetTokenHasher resetTokenHasher,
        User user,
        Credential credential,
        PasswordReset? existingReset)
    {
        return new CompletePasswordResetCommandHandler(
            new FakeUserRepository(user),
            new FakePasswordResetRepository(existingReset),
            resetTokenHasher,
            new FakeCredentialRepository(credential),
            passwordHasher,
            new FakeAuthenticationUnitOfWork());
    }

    private static void AssertGenericFailure(ExecutionResult result)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(GenericErrorCode, result.ErrorCode);
    }

    private static User CreateUser()
    {
        return User.Create(
            UserCode.Create("USR-COMPLETERESET-001"),
            IdentityProfileId.New(),
            Username.Create("complete-reset-test-user"));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly User _user;

        public FakeUserRepository(User user)
        {
            _user = user;
        }

        public Task<User?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_user.Username == username ? _user : null);
        }

        public Task<Guid?> GetLinkedPersonIdAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Guid?>(null);
        }
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

    private sealed class FakePasswordResetRepository : IPasswordResetRepository
    {
        private readonly object _gate = new();
        private PasswordReset? _reset;

        public FakePasswordResetRepository(PasswordReset? reset)
        {
            _reset = reset;
        }

        public void Add(PasswordReset passwordReset)
        {
            lock (_gate)
            {
                _reset = passwordReset;
            }
        }

        public Task<PasswordReset?> GetPendingByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                var reset = _reset;
                if (reset is null || reset.UserId != userId || reset.Status != PasswordResetStatus.Pending)
                {
                    return Task.FromResult<PasswordReset?>(null);
                }

                return Task.FromResult<PasswordReset?>(reset);
            }
        }

        public Task<bool> TryCompleteAsync(
            PasswordResetId id,
            DateTime completedAtUtc,
            CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                if (_reset is null || _reset.Id != id || _reset.Status != PasswordResetStatus.Pending)
                {
                    return Task.FromResult(false);
                }

                _reset.Complete(completedAtUtc);
                return Task.FromResult(true);
            }
        }
    }

    private sealed class FakeAuthenticationUnitOfWork : IAuthenticationUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
