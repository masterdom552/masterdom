using Masterdom.Core.Identity.Entities.Credential;
using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Security;
using Masterdom.Modules.Authentication.Application.Commands;
using Masterdom.Modules.Authentication.Application.Handlers;
using Masterdom.Modules.Authentication.Application.Models;
using Masterdom.Modules.Authentication.Application.Services;

namespace Masterdom.Core.Tests.Authentication;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldSucceedAndIssueToken()
    {
        var (handler, user, _) = CreateHandlerWithSeededUser("correct-password");

        var result = await handler.HandleAsync(new LoginCommand(user.Username.Value, "correct-password"));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(string.IsNullOrWhiteSpace(result.Value!.AccessToken));
    }

    [Fact]
    public async Task HandleAsync_WithWrongPassword_ShouldFailGenerically()
    {
        var (handler, user, _) = CreateHandlerWithSeededUser("correct-password");

        var result = await handler.HandleAsync(new LoginCommand(user.Username.Value, "wrong-password"));

        Assert.False(result.IsSuccess);
        Assert.Equal("unauthorized", result.ErrorCode);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownUsername_ShouldFailWithSameGenericError()
    {
        var (handler, _, _) = CreateHandlerWithSeededUser("correct-password");

        var result = await handler.HandleAsync(new LoginCommand("nonexistent-user", "correct-password"));

        Assert.False(result.IsSuccess);
        Assert.Equal("unauthorized", result.ErrorCode);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveUser_ShouldFailWithSameGenericError()
    {
        var (handler, user, userRepository) = CreateHandlerWithSeededUser("correct-password");
        user.Deactivate();
        userRepository.Save(user);

        var result = await handler.HandleAsync(new LoginCommand(user.Username.Value, "correct-password"));

        Assert.False(result.IsSuccess);
        Assert.Equal("unauthorized", result.ErrorCode);
    }

    [Fact]
    public async Task HandleAsync_FailureResult_ShouldNeverExposePasswordOrHash()
    {
        var (handler, user, _) = CreateHandlerWithSeededUser("correct-password");

        var result = await handler.HandleAsync(new LoginCommand(user.Username.Value, "wrong-password"));

        Assert.Null(result.Value);
        Assert.DoesNotContain("correct-password", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_SuccessResult_ShouldOnlyExposeTokenAndExpiry()
    {
        var (handler, user, _) = CreateHandlerWithSeededUser("correct-password");

        var result = await handler.HandleAsync(new LoginCommand(user.Username.Value, "correct-password"));

        var properties = typeof(LoginResult).GetProperties().Select(p => p.Name).ToArray();
        Assert.Equal(["AccessToken", "ExpiresAtUtc"], properties);
    }

    [Fact]
    public async Task HandleAsync_WithResolvedAuthority_ShouldEmbedItInTheIssuedToken()
    {
        var (handler, user, _) = CreateHandlerWithSeededUser(
            "correct-password",
            authorityClaims: new LoginAuthorityClaims(
                RoleCodes: ["SUPERUSER"],
                Permissions: [],
                PropertyScopes: [],
                AuthorityLevel: AuthorityLevels.PrimarySuperUser));

        var result = await handler.HandleAsync(new LoginCommand(user.Username.Value, "correct-password"));

        Assert.True(result.IsSuccess);
        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .ReadJwtToken(result.Value!.AccessToken);

        Assert.Contains(
            token.Claims,
            c => c.Type == MasterdomClaimTypes.AuthorityLevel
                && c.Value == AuthorityLevels.PrimarySuperUser.ToString());
    }

    private static (LoginCommandHandler Handler, User User, FakeUserRepository UserRepository) CreateHandlerWithSeededUser(
        string password,
        LoginAuthorityClaims? authorityClaims = null)
    {
        var identityProfileId = IdentityProfileId.New();
        var user = User.Create(
            UserCode.Create("USR-LOGIN-001"),
            identityProfileId,
            Username.Create("login-test-user"));

        var passwordHasher = new PasswordHasher();
        var credential = Credential.Create(user.Id, passwordHasher.Hash(password));

        var userRepository = new FakeUserRepository(user);
        var credentialRepository = new FakeCredentialRepository(credential);
        var propertyOwnershipProvider = new FakePropertyOwnershipProvider();
        var loginAuthorityResolver = new FakeLoginAuthorityResolver(authorityClaims);
        var jwtTokenIssuer = new JwtTokenIssuer(new JwtTokenIssuerOptions
        {
            SigningKey = "test-signing-key-that-is-sufficiently-long",
        });

        var handler = new LoginCommandHandler(
            userRepository,
            credentialRepository,
            passwordHasher,
            propertyOwnershipProvider,
            loginAuthorityResolver,
            jwtTokenIssuer);

        return (handler, user, userRepository);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private User _user;

        public FakeUserRepository(User user)
        {
            _user = user;
        }

        public void Save(User user)
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

    private sealed class FakePropertyOwnershipProvider : IPropertyOwnershipProvider
    {
        public Task<IReadOnlyCollection<Guid>> GetOwnedPropertyIdsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Guid>>([]);
        }
    }

    private sealed class FakeLoginAuthorityResolver : ILoginAuthorityResolver
    {
        private readonly LoginAuthorityClaims? _fixedClaims;

        public FakeLoginAuthorityResolver(LoginAuthorityClaims? fixedClaims = null)
        {
            _fixedClaims = fixedClaims;
        }

        public Task<LoginAuthorityClaims> ResolveAsync(
            Guid userId,
            IReadOnlyCollection<Guid> directPropertyScopes,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_fixedClaims ?? LoginAuthorityClaims.None(directPropertyScopes));
        }
    }
}
