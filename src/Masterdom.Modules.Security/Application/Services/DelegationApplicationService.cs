using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.ValueObjects;
using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence.Identity;
using Masterdom.Modules.Security.Application.Commands;
using Masterdom.Modules.Security.Application.Support;

namespace Masterdom.Modules.Security.Application.Services;

/// <summary>
/// Application service for delegation management.
///
/// Orchestrates:
/// - Obtaining the authenticated user's effective authority
/// - Validating delegation constraints (non-escalation, scope containment, temporal bounds)
/// - Persisting delegations through the repository
/// </summary>
public sealed class DelegationApplicationService : IDelegationApplicationService
{
    private readonly IDelegatedAuthorityRepository _delegatedAuthorityRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly EffectiveAuthorityResolver _effectiveAuthorityResolver;
    private readonly DelegationValidator _delegationValidator;
    private readonly IAuthorityLevelProvider _authorityLevelProvider;
    private readonly IDirectAuthorityProvider _directAuthorityProvider;
    private readonly IIdentityAdministrationUnitOfWork _unitOfWork;

    public DelegationApplicationService(
        IDelegatedAuthorityRepository delegatedAuthorityRepository,
        ICurrentUserAccessor currentUserAccessor,
        EffectiveAuthorityResolver effectiveAuthorityResolver,
        DelegationValidator delegationValidator,
        IAuthorityLevelProvider authorityLevelProvider,
        IDirectAuthorityProvider directAuthorityProvider,
        IIdentityAdministrationUnitOfWork unitOfWork)
    {
        _delegatedAuthorityRepository = delegatedAuthorityRepository ??
            throw new ArgumentNullException(nameof(delegatedAuthorityRepository));
        _currentUserAccessor = currentUserAccessor ??
            throw new ArgumentNullException(nameof(currentUserAccessor));
        _effectiveAuthorityResolver = effectiveAuthorityResolver ??
            throw new ArgumentNullException(nameof(effectiveAuthorityResolver));
        _delegationValidator = delegationValidator ??
            throw new ArgumentNullException(nameof(delegationValidator));
        _authorityLevelProvider = authorityLevelProvider ??
            throw new ArgumentNullException(nameof(authorityLevelProvider));
        _directAuthorityProvider = directAuthorityProvider ??
            throw new ArgumentNullException(nameof(directAuthorityProvider));
        _unitOfWork = unitOfWork ??
            throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Creates a new delegation.
    ///
    /// Security:
    /// - Delegator is always the authenticated user (client cannot supply delegator)
    /// - Validates that delegator's effective authority permits the delegation
    /// - Enforces non-escalation: delegatee authority must not exceed delegator's
    /// - Enforces scope containment: delegatee scope must be subset of delegator's
    /// - Enforces temporal containment: child period must be within parent period
    /// </summary>
    public async Task<DelegatedAuthority> CreateDelegationAsync(CreateDelegationCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Get the authenticated user - this is the delegator
        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            throw new InvalidOperationException(
                "User must be authenticated to create a delegation.");
        }

        var delegatorUserId = new UserId(currentUser.UserId.Value);
        var delegateeUserId = new UserId(command.DelegateeUserId);
        var delegatedRoleId = new RoleId(command.DelegatedRoleId);

        // Build the delegation scope from property IDs
        var scope = command.PropertyIds.Length > 0
            ? DelegationScope.WithProperties(command.PropertyIds)
            : DelegationScope.Unrestricted();

        // Load delegator's active delegations to compute effective authority
        var activeDelegations = await _delegatedAuthorityRepository
            .GetActiveDelegationsAsync(currentUser.UserId.Value, DateTime.UtcNow);

        // Load the delegator's direct authority from the authoritative identity model
        var directAuthority = await _directAuthorityProvider.GetDirectAuthorityAsync(
            currentUser.UserId.Value,
            currentUser.PropertyScopes,
            cancellationToken);

        if (directAuthority == null)
        {
            throw new InvalidOperationException(
                "User has no active primary role assignment. Cannot delegate authority.");
        }

        // Compute delegator's effective authority
        var delegatorEffectiveAuthority = _effectiveAuthorityResolver.Resolve(
            currentUser.UserId.Value,
            directAuthority,
            activeDelegations,
            DateTime.UtcNow);

        // Build delegation proposal
        var proposal = new DelegationProposal(
            delegatorUserId.Value,
            delegateeUserId.Value,
            delegatedRoleId.Value,
            scope,
            command.EffectiveFromUtc,
            command.EffectiveToUtc);

        // Validate delegation constraints (domain rules)
        var validationResult = _delegationValidator.Validate(proposal, delegatorEffectiveAuthority);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException(validationResult.ErrorMessage);
        }

        // Create the delegation aggregate (domain encapsulation)
        var delegation = DelegatedAuthority.Create(
            delegatorUserId,
            delegateeUserId,
            delegatedRoleId,
            scope,
            command.EffectiveFromUtc,
            command.EffectiveToUtc);

        // Set optional fields
        if (!string.IsNullOrWhiteSpace(command.Description))
        {
            delegation.ChangeDescription(command.Description);
        }

        if (!string.IsNullOrWhiteSpace(command.Remarks))
        {
            delegation.ChangeRemarks(command.Remarks);
        }

        // Persist
        await Task.Run(() =>
        {
            _unitOfWork.Execute(() =>
            {
                _delegatedAuthorityRepository.Add(delegation);
            });
        }, cancellationToken);

        return delegation;
    }

    /// <summary>
    /// Revokes an existing delegation.
    ///
    /// Security:
    /// - Acting user is always the authenticated user
    /// - Only the delegator or higher authority can revoke
    /// - Cannot revoke an already-revoked delegation
    /// </summary>
    public DelegatedAuthority RevokeDelegation(RevokeDelegationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Get the authenticated user - this is the revoker
        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            throw new InvalidOperationException(
                "User must be authenticated to revoke a delegation.");
        }

        var revokedBy = new UserId(currentUser.UserId.Value);

        // Load the delegation
        var delegatedAuthorityId = new DelegatedAuthorityId(command.DelegatedAuthorityId);
        var delegation = _delegatedAuthorityRepository.GetByIdAsync(delegatedAuthorityId).Result;

        if (delegation is null)
        {
            throw new InvalidOperationException(
                $"Delegation with ID '{delegatedAuthorityId}' not found.");
        }

        // Verify that the acting user has authority to revoke it
        // Currently: only the delegator can revoke (future: support higher authority)
        if (delegation.DelegatorUserId != revokedBy && !currentUser.IsInherentSuperUser)
        {
            throw new InvalidOperationException(
                "Only the delegator or primary authority can revoke a delegation.");
        }

        // Revoke the delegation (domain logic)
        delegation.Revoke(revokedBy, command.RevocationReason);

        // Persist
        _unitOfWork.Execute(() =>
        {
            _delegatedAuthorityRepository.Update(delegation);
        });

        return delegation;
    }

    /// <summary>
    /// Gets a delegation by ID.
    /// </summary>
    public async Task<DelegatedAuthority?> GetDelegationByIdAsync(Guid delegatedAuthorityId)
    {
        var id = new DelegatedAuthorityId(delegatedAuthorityId);
        return await _delegatedAuthorityRepository.GetByIdAsync(id);
    }
}

