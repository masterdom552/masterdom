using Masterdom.Core.Identity.Entities.Credential;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Security;
using Masterdom.Modules.Authentication.Application.Commands;
using Masterdom.Modules.Authentication.Application.Models;
using Masterdom.Modules.Authentication.Application.Services;
using Masterdom.Modules.Authentication.Application.Support;

namespace Masterdom.Modules.Authentication.Application.Handlers;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, ExecutionResult<LoginResult>>
{
    private const string GenericFailureMessage = "Invalid username or password.";

    private readonly IUserRepository _userRepository;
    private readonly ICredentialRepository _credentialRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPropertyOwnershipProvider _propertyOwnershipProvider;
    private readonly ILoginAuthorityResolver _loginAuthorityResolver;
    private readonly IJwtTokenIssuer _jwtTokenIssuer;

    public LoginCommandHandler(
        IUserRepository userRepository,
        ICredentialRepository credentialRepository,
        IPasswordHasher passwordHasher,
        IPropertyOwnershipProvider propertyOwnershipProvider,
        ILoginAuthorityResolver loginAuthorityResolver,
        IJwtTokenIssuer jwtTokenIssuer)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _propertyOwnershipProvider = propertyOwnershipProvider ?? throw new ArgumentNullException(nameof(propertyOwnershipProvider));
        _loginAuthorityResolver = loginAuthorityResolver ?? throw new ArgumentNullException(nameof(loginAuthorityResolver));
        _jwtTokenIssuer = jwtTokenIssuer ?? throw new ArgumentNullException(nameof(jwtTokenIssuer));
    }

    public async Task<ExecutionResult<LoginResult>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Username username;
        try
        {
            username = Username.Create(command.Username);
        }
        catch (ArgumentException)
        {
            return Fail();
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            return Fail();
        }

        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (user is null || user.Status != UserStatus.Active)
        {
            return Fail();
        }

        var credential = await _credentialRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (credential is null || credential.Status != CredentialStatus.Active)
        {
            return Fail();
        }

        if (!_passwordHasher.Verify(credential.PasswordHash, command.Password))
        {
            return Fail();
        }

        var ownedPropertyIds = await _propertyOwnershipProvider.GetOwnedPropertyIdsAsync(
            user.Id.Value,
            cancellationToken);

        var personId = await _userRepository.GetLinkedPersonIdAsync(user.Id, cancellationToken);

        var authorityClaims = await _loginAuthorityResolver.ResolveAsync(
            user.Id.Value,
            ownedPropertyIds,
            cancellationToken);

        var loginResult = _jwtTokenIssuer.Issue(
            user.Id.Value,
            user.Username.Value,
            personId,
            ownedPropertyIds,
            authorityClaims);

        return ExecutionResult<LoginResult>.Success(loginResult);
    }

    private static ExecutionResult<LoginResult> Fail()
    {
        return ExecutionResult<LoginResult>.Failure("unauthorized", GenericFailureMessage);
    }
}
