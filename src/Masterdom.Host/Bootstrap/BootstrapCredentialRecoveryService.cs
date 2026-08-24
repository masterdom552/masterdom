using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Host.Bootstrap;

/// <summary>
/// Input for the bootstrap credential recovery operation.
/// </summary>
public sealed record BootstrapCredentialRecoveryRequest(
    string Username,
    string NewPassword,
    string RecoverySecret);

/// <summary>
/// Result of a bootstrap credential recovery attempt. Never carries the
/// plaintext new password, recovery secret, or any password hash.
/// </summary>
public sealed record BootstrapCredentialRecoveryResult(bool Success, string Message, Guid? UserId = null);

/// <summary>
/// Recovers the credential of the existing, bootstrap-provisioned
/// <see cref="RoleAuthorityLevel.PrimarySuperUser"/> identity when its
/// password has been lost and no authenticated or privileged actor exists
/// to use CAP-023's ordinary self-service or administrator-mediated
/// password-reset flows.
///
/// This is a narrow, explicitly operator-invoked, secret-gated recovery
/// mechanism -- not a general credential-administration surface. It never
/// creates a User, Person, IdentityProfile, Role, or UserRole; it mutates
/// only the existing Credential of the explicitly named target user, after
/// verifying that user actually holds the PrimarySuperUser authority level.
/// It never mints a JWT; the recovered identity authenticates afterward
/// through the existing, unmodified CAP-023 login flow.
/// </summary>
public sealed class BootstrapCredentialRecoveryService
{
    private const int MinimumRecoverySecretLength = 16;
    private const int MinimumPasswordLength = 8;

    private readonly MasterdomDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly ICredentialRepository _credentialRepository;
    private readonly IPasswordHasher _passwordHasher;

    public BootstrapCredentialRecoveryService(
        MasterdomDbContext dbContext,
        IUserRepository userRepository,
        ICredentialRepository credentialRepository,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<BootstrapCredentialRecoveryResult> RecoverAsync(
        BootstrapCredentialRecoveryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.RecoverySecret) ||
            request.RecoverySecret.Length < MinimumRecoverySecretLength)
        {
            return new BootstrapCredentialRecoveryResult(
                false,
                "Bootstrap credential recovery secret was not supplied or is too short.");
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return new BootstrapCredentialRecoveryResult(
                false,
                "Recovery username was not supplied.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < MinimumPasswordLength)
        {
            return new BootstrapCredentialRecoveryResult(
                false,
                $"Recovery password was not supplied or is too short (minimum {MinimumPasswordLength} characters).");
        }

        Username username;
        try
        {
            username = Username.Create(request.Username);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }

        try
        {
            var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
            if (user is null)
            {
                return NotFound();
            }

            var isPrimarySuperUser = await IsPrimarySuperUserAsync(user.Id, cancellationToken);
            if (!isPrimarySuperUser)
            {
                return new BootstrapCredentialRecoveryResult(
                    false,
                    "The specified user does not hold the PrimarySuperUser authority level.");
            }

            var credential = await _credentialRepository.GetByUserIdAsync(user.Id, cancellationToken);
            if (credential is null)
            {
                return new BootstrapCredentialRecoveryResult(
                    false,
                    "The specified user has no credential to recover.");
            }

            var newPasswordHash = _passwordHasher.Hash(request.NewPassword);
            credential.ChangePassword(newPasswordHash);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new BootstrapCredentialRecoveryResult(
                true,
                "Bootstrap credential recovery completed successfully.",
                user.Id.Value);
        }
        catch (Exception ex)
        {
            return new BootstrapCredentialRecoveryResult(false, $"Bootstrap credential recovery failed: {ex.Message}");
        }
    }

    private async Task<bool> IsPrimarySuperUserAsync(UserId userId, CancellationToken cancellationToken)
    {
        var userRoles = await _dbContext.UserRoles
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var effectivePrimaryRoleIds = userRoles
            .Where(ur => ur.UserId == userId && ur.IsPrimaryRole && ur.IsEffective(now))
            .Select(ur => ur.RoleId)
            .ToHashSet();

        if (effectivePrimaryRoleIds.Count == 0)
        {
            return false;
        }

        var roles = await _dbContext.Roles
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return roles.Any(r =>
            effectivePrimaryRoleIds.Contains(r.Id) &&
            r.AuthorityLevel == RoleAuthorityLevel.PrimarySuperUser);
    }

    private static BootstrapCredentialRecoveryResult NotFound()
    {
        return new BootstrapCredentialRecoveryResult(false, "No user was found with the specified username.");
    }
}
