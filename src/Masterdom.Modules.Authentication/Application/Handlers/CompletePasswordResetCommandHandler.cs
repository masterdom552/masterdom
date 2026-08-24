using Masterdom.Core.Identity.Entities.Credential;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Security;
using Masterdom.Modules.Authentication.Application.Commands;
using Masterdom.Modules.Authentication.Application.Services;
using Masterdom.Modules.Authentication.Application.Support;

namespace Masterdom.Modules.Authentication.Application.Handlers;

/// <summary>
/// Anonymous password reset redemption -- the one anonymous surface this
/// package adds. Every invalid case (unknown username, no pending reset,
/// expired, wrong token) returns the identical generic failure, matching
/// the anti-enumeration principle already established by
/// <see cref="LoginCommandHandler"/>.
/// </summary>
public sealed class CompletePasswordResetCommandHandler
    : ICommandHandler<CompletePasswordResetCommand, ExecutionResult>
{
    private const int MinimumPasswordLength = 8;
    private const string GenericFailureMessage = "The password reset request is invalid or has expired.";

    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetRepository _passwordResetRepository;
    private readonly IResetTokenHasher _resetTokenHasher;
    private readonly ICredentialRepository _credentialRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthenticationUnitOfWork _unitOfWork;

    public CompletePasswordResetCommandHandler(
        IUserRepository userRepository,
        IPasswordResetRepository passwordResetRepository,
        IResetTokenHasher resetTokenHasher,
        ICredentialRepository credentialRepository,
        IPasswordHasher passwordHasher,
        IAuthenticationUnitOfWork unitOfWork)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordResetRepository = passwordResetRepository ?? throw new ArgumentNullException(nameof(passwordResetRepository));
        _resetTokenHasher = resetTokenHasher ?? throw new ArgumentNullException(nameof(resetTokenHasher));
        _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ExecutionResult> HandleAsync(
        CompletePasswordResetCommand command,
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

        if (string.IsNullOrWhiteSpace(command.Token))
        {
            return Fail();
        }

        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (user is null)
        {
            return Fail();
        }

        var passwordReset = await _passwordResetRepository.GetPendingByUserIdAsync(user.Id, cancellationToken);
        if (passwordReset is null)
        {
            return Fail();
        }

        var now = DateTime.UtcNow;
        if (!passwordReset.IsValid(now))
        {
            return Fail();
        }

        if (!_resetTokenHasher.Verify(passwordReset.TokenHash, command.Token))
        {
            return Fail();
        }

        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < MinimumPasswordLength)
        {
            return ExecutionResult.Failure(
                "validation_failed",
                $"The new password must be at least {MinimumPasswordLength} characters.");
        }

        var credential = await _credentialRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (credential is null || credential.Status != CredentialStatus.Active)
        {
            return Fail();
        }

        var claimed = await _passwordResetRepository.TryCompleteAsync(passwordReset.Id, now, cancellationToken);
        if (!claimed)
        {
            return Fail();
        }

        var newPasswordHash = _passwordHasher.Hash(command.NewPassword);
        credential.ChangePassword(newPasswordHash);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ExecutionResult.Success();
    }

    private static ExecutionResult Fail()
    {
        return ExecutionResult.Failure("unauthorized", GenericFailureMessage);
    }
}
