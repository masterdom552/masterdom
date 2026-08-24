using Masterdom.Core.Identity.Entities.PasswordReset;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Security;
using Masterdom.Modules.Authentication.Application.Commands;
using Masterdom.Modules.Authentication.Application.Models;
using Masterdom.Modules.Authentication.Application.Services;
using Masterdom.Modules.Authentication.Application.Support;

namespace Masterdom.Modules.Authentication.Application.Handlers;

/// <summary>
/// Administrator-mediated password reset. Authorization is the existing,
/// server-derived <see cref="CurrentUser.IsInherentSuperUser"/> claim -- no
/// parallel authorization mechanism.
/// </summary>
public sealed class RequestPasswordResetCommandHandler
    : ICommandHandler<RequestPasswordResetCommand, ExecutionResult<RequestPasswordResetResult>>
{
    private static readonly TimeSpan ResetLifetime = TimeSpan.FromMinutes(15);

    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetRepository _passwordResetRepository;
    private readonly IResetTokenHasher _resetTokenHasher;
    private readonly IAuthenticationUnitOfWork _unitOfWork;

    public RequestPasswordResetCommandHandler(
        ICurrentUserAccessor currentUserAccessor,
        IUserRepository userRepository,
        IPasswordResetRepository passwordResetRepository,
        IResetTokenHasher resetTokenHasher,
        IAuthenticationUnitOfWork unitOfWork)
    {
        _currentUserAccessor = currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordResetRepository = passwordResetRepository ?? throw new ArgumentNullException(nameof(passwordResetRepository));
        _resetTokenHasher = resetTokenHasher ?? throw new ArgumentNullException(nameof(resetTokenHasher));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ExecutionResult<RequestPasswordResetResult>> HandleAsync(
        RequestPasswordResetCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (!currentUser.IsAuthenticated)
        {
            return ExecutionResult<RequestPasswordResetResult>.Failure(
                "unauthorized",
                "The caller is not authenticated.");
        }

        if (!currentUser.IsInherentSuperUser)
        {
            return ExecutionResult<RequestPasswordResetResult>.Failure(
                "forbidden",
                "Only a primary administrator can initiate a password reset for another user.");
        }

        Username targetUsername;
        try
        {
            targetUsername = Username.Create(command.TargetUsername);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }

        var targetUser = await _userRepository.GetByUsernameAsync(targetUsername, cancellationToken);
        if (targetUser is null)
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;

        var existingPending = await _passwordResetRepository.GetPendingByUserIdAsync(targetUser.Id, cancellationToken);
        existingPending?.Cancel(now);

        var token = _resetTokenHasher.GenerateToken();
        var tokenHash = _resetTokenHasher.Hash(token);

        var passwordReset = PasswordReset.Create(targetUser.Id, tokenHash, ResetLifetime);
        _passwordResetRepository.Add(passwordReset);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ExecutionResult<RequestPasswordResetResult>.Success(
            new RequestPasswordResetResult(token, passwordReset.ExpiresAtUtc));
    }

    private static ExecutionResult<RequestPasswordResetResult> NotFound()
    {
        return ExecutionResult<RequestPasswordResetResult>.Failure(
            "not_found",
            "No user was found with the specified username.");
    }
}
