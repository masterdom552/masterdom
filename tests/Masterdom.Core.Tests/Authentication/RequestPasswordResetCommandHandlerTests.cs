using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Core.Identity.Entities.PasswordReset;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Security;
using Masterdom.Modules.Authentication.Application.Commands;
using Masterdom.Modules.Authentication.Application.Handlers;
using Masterdom.Modules.Authentication.Application.Services;
using Masterdom.Modules.Authentication.Application.Support;

namespace Masterdom.Core.Tests.Authentication;

public sealed class RequestPasswordResetCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithSuperUserCaller_ShouldCreatePendingResetAndReturnTokenOnce()
    {
        var user = CreateUser();
        var userRepository = new FakeUserRepository(user);
        var passwordResetRepository = new FakePasswordResetRepository();

        var handler = CreateHandler(isInherentSuperUser: true, userRepository, passwordResetRepository);

        var result = await handler.HandleAsync(new RequestPasswordResetCommand(user.Username.Value));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(string.IsNullOrWhiteSpace(result.Value!.ResetToken));

        var pending = await passwordResetRepository.GetPendingByUserIdAsync(user.Id);
        Assert.NotNull(pending);
        Assert.Equal(PasswordResetStatus.Pending, pending!.Status);
    }

    [Fact]
    public async Task HandleAsync_WithSuperUserCaller_ShouldNotPersistThePlaintextToken()
    {
        var user = CreateUser();
        var userRepository = new FakeUserRepository(user);
        var passwordResetRepository = new FakePasswordResetRepository();

        var handler = CreateHandler(isInherentSuperUser: true, userRepository, passwordResetRepository);

        var result = await handler.HandleAsync(new RequestPasswordResetCommand(user.Username.Value));

        var pending = await passwordResetRepository.GetPendingByUserIdAsync(user.Id);
        Assert.NotNull(pending);
        Assert.DoesNotContain(result.Value!.ResetToken, pending!.TokenHash);
        Assert.NotEqual(result.Value.ResetToken, pending.TokenHash);
    }

    [Fact]
    public async Task HandleAsync_WithNonSuperUserCaller_ShouldFailForbidden()
    {
        var user = CreateUser();
        var userRepository = new FakeUserRepository(user);
        var passwordResetRepository = new FakePasswordResetRepository();

        var handler = CreateHandler(isInherentSuperUser: false, userRepository, passwordResetRepository);

        var result = await handler.HandleAsync(new RequestPasswordResetCommand(user.Username.Value));

        Assert.False(result.IsSuccess);
        Assert.Equal("forbidden", result.ErrorCode);
    }

    [Fact]
    public async Task HandleAsync_WithUnauthenticatedCaller_ShouldFailUnauthorized()
    {
        var user = CreateUser();
        var userRepository = new FakeUserRepository(user);
        var passwordResetRepository = new FakePasswordResetRepository();

        var handler = new RequestPasswordResetCommandHandler(
            new FakeCurrentUserAccessor(CurrentUser.Anonymous),
            userRepository,
            passwordResetRepository,
            new ResetTokenHasher(),
            new FakeAuthenticationUnitOfWork());

        var result = await handler.HandleAsync(new RequestPasswordResetCommand(user.Username.Value));

        Assert.False(result.IsSuccess);
        Assert.Equal("unauthorized", result.ErrorCode);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownTargetUsername_ShouldFailNotFound()
    {
        var user = CreateUser();
        var userRepository = new FakeUserRepository(user);
        var passwordResetRepository = new FakePasswordResetRepository();

        var handler = CreateHandler(isInherentSuperUser: true, userRepository, passwordResetRepository);

        var result = await handler.HandleAsync(new RequestPasswordResetCommand("nonexistent-user"));

        Assert.False(result.IsSuccess);
        Assert.Equal("not_found", result.ErrorCode);
    }

    [Fact]
    public async Task HandleAsync_WithExistingPendingReset_ShouldCancelPriorAndLeaveOnlyOnePending()
    {
        var user = CreateUser();
        var userRepository = new FakeUserRepository(user);
        var passwordResetRepository = new FakePasswordResetRepository();

        var handler = CreateHandler(isInherentSuperUser: true, userRepository, passwordResetRepository);

        var first = await handler.HandleAsync(new RequestPasswordResetCommand(user.Username.Value));
        var second = await handler.HandleAsync(new RequestPasswordResetCommand(user.Username.Value));

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);

        var pendingResets = passwordResetRepository.AllForUser(user.Id)
            .Where(r => r.Status == PasswordResetStatus.Pending)
            .ToArray();
        Assert.Single(pendingResets);

        var cancelledResets = passwordResetRepository.AllForUser(user.Id)
            .Where(r => r.Status == PasswordResetStatus.Cancelled)
            .ToArray();
        Assert.Single(cancelledResets);
    }

    private static RequestPasswordResetCommandHandler CreateHandler(
        bool isInherentSuperUser,
        FakeUserRepository userRepository,
        FakePasswordResetRepository passwordResetRepository)
    {
        var currentUser = CurrentUser.Authenticated(
            Guid.CreateVersion7(),
            personId: null,
            "admin-user",
            roles: [],
            permissions: [],
            propertyScopes: [],
            ownedPropertyIds: [],
            isInherentSuperUser: isInherentSuperUser);

        return new RequestPasswordResetCommandHandler(
            new FakeCurrentUserAccessor(currentUser),
            userRepository,
            passwordResetRepository,
            new ResetTokenHasher(),
            new FakeAuthenticationUnitOfWork());
    }

    private static User CreateUser()
    {
        return User.Create(
            UserCode.Create("USR-REQRESET-001"),
            IdentityProfileId.New(),
            Username.Create("request-reset-test-user"));
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

    private sealed class FakePasswordResetRepository : IPasswordResetRepository
    {
        private readonly List<PasswordReset> _resets = [];

        public void Add(PasswordReset passwordReset)
        {
            _resets.Add(passwordReset);
        }

        public Task<PasswordReset?> GetPendingByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            var pending = _resets
                .Where(r => r.UserId == userId && r.Status == PasswordResetStatus.Pending)
                .OrderByDescending(r => r.RequestedAtUtc)
                .FirstOrDefault();

            return Task.FromResult(pending);
        }

        public Task<bool> TryCompleteAsync(
            PasswordResetId id,
            DateTime completedAtUtc,
            CancellationToken cancellationToken = default)
        {
            var reset = _resets.SingleOrDefault(r => r.Id == id);
            if (reset is null || reset.Status != PasswordResetStatus.Pending)
            {
                return Task.FromResult(false);
            }

            reset.Complete(completedAtUtc);
            return Task.FromResult(true);
        }

        public IReadOnlyCollection<PasswordReset> AllForUser(UserId userId)
        {
            return _resets.Where(r => r.UserId == userId).ToArray();
        }
    }

    private sealed class FakeAuthenticationUnitOfWork : IAuthenticationUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
