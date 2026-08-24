using Masterdom.Core.Identity.Entities.Credential;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Security;
using Masterdom.Modules.Authentication.Application.Commands;
using Masterdom.Modules.Authentication.Application.Support;

namespace Masterdom.Modules.Authentication.Application.Handlers;

/// <summary>
/// Authenticated self-service password change. The acting user is resolved
/// exclusively from <see cref="ICurrentUserAccessor"/> -- never from the
/// command payload -- so client input cannot determine whose password is
/// changed.
/// </summary>
public sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, ExecutionResult>
{
    private const int MinimumPasswordLength = 8;
    private const string GenericFailureMessage = "The current password is incorrect.";

    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ICredentialRepository _credentialRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthenticationUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(
        ICurrentUserAccessor currentUserAccessor,
        ICredentialRepository credentialRepository,
        IPasswordHasher passwordHasher,
        IAuthenticationUnitOfWork unitOfWork)
    {
        _currentUserAccessor = currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
        _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ExecutionResult> HandleAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return ExecutionResult.Failure("unauthorized", "The caller is not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.CurrentPassword))
        {
            return ExecutionResult.Failure("unauthorized", GenericFailureMessage);
        }

        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < MinimumPasswordLength)
        {
            return ExecutionResult.Failure(
                "validation_failed",
                $"The new password must be at least {MinimumPasswordLength} characters.");
        }

        var userId = UserId.From(currentUser.UserId.Value);

        var credential = await _credentialRepository.GetByUserIdAsync(userId, cancellationToken);
        if (credential is null || credential.Status != CredentialStatus.Active)
        {
            return ExecutionResult.Failure("unauthorized", GenericFailureMessage);
        }

        if (!_passwordHasher.Verify(credential.PasswordHash, command.CurrentPassword))
        {
            return ExecutionResult.Failure("unauthorized", GenericFailureMessage);
        }

        var newPasswordHash = _passwordHasher.Hash(command.NewPassword);
        credential.ChangePassword(newPasswordHash);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ExecutionResult.Success();
    }
}
