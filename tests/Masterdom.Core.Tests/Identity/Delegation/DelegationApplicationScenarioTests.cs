using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.ValueObjects;
using Masterdom.Core.Security;
using Xunit;

namespace Masterdom.Core.Tests.Identity.Delegation;

/// <summary>
/// Delegation Application Scenario Tests
/// Tests the core delegation workflow through realistic scenarios.
/// </summary>
public sealed class DelegationApplicationScenarioTests
{
    private readonly UserId _delegatorUserId = new(Guid.NewGuid());
    private readonly UserId _delegateeUserId = new(Guid.NewGuid());
    private readonly RoleId _roleId = new(Guid.NewGuid());
    private readonly DelegationScope _scope = DelegationScope.Unrestricted();
    private readonly DateTime _now = DateTime.UtcNow;

    [Fact]
    public void CreateDelegation_WithPropertyScope_StoresScopeCorrectly()
    {
        // Arrange
        var propertyIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var scope = DelegationScope.WithProperties(propertyIds);

        // Act
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            scope,
            _now,
            _now.AddMonths(1));

        // Assert
        Assert.NotNull(delegation.Scope);
        Assert.NotNull(delegation.Scope.PropertyIds);
        Assert.Equal(2, delegation.Scope.PropertyIds.Count);
    }

    [Fact]
    public void CreateDelegation_DelegationWithinSingleDay_Allowed()
    {
        // Arrange
        var oneHourLater = _now.AddHours(1);

        // Act
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            _now,
            oneHourLater);

        // Assert
        Assert.NotNull(delegation);
        Assert.Equal(DelegatedAuthorityStatus.Active, delegation.Status);
    }

    [Fact]
    public void RevokeDelegation_UpdatesMetadataCorrectly()
    {
        // Arrange
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            _now,
            _now.AddMonths(1));

        var revokerUserId = new UserId(Guid.NewGuid());
        var revokeReason = "Test revocation for audit trail";

        // Act
        delegation.Revoke(revokerUserId, revokeReason);

        // Assert
        Assert.Equal(DelegatedAuthorityStatus.Revoked, delegation.Status);
        Assert.Equal(revokerUserId, delegation.RevokedBy);
        Assert.NotNull(delegation.RevokedAtUtc);
        Assert.Equal(revokeReason, delegation.RevocationReason);
    }

    [Fact]
    public void DelegationAggregate_CanChangeDescription()
    {
        // Arrange
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            _now,
            _now.AddMonths(1));

        var newDescription = "Updated delegation description for property management";

        // Act
        delegation.ChangeDescription(newDescription);

        // Assert
        Assert.Equal(newDescription, delegation.Description);
    }

    [Fact]
    public void DelegationAggregate_CanChangeRemarks()
    {
        // Arrange
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            _now,
            _now.AddMonths(1));

        var remarks = "Internal remarks about this delegation for tracking";

        // Act
        delegation.ChangeRemarks(remarks);

        // Assert
        Assert.Equal(remarks, delegation.Remarks);
    }

    [Fact]
    public void DelegationAggregate_TemporallyExpired_StillIsPersistable()
    {
        // Arrange - create delegation that expired in the past
        var pastStart = _now.AddDays(-2);
        var pastEnd = _now.AddDays(-1);

        // Act
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            pastStart,
            pastEnd);

        // Assert
        Assert.NotNull(delegation);
        Assert.Equal(DelegatedAuthorityStatus.Active, delegation.Status);
    }

    [Fact]
    public void DelegationAggregate_SameDelegatorAndDelegatee_Allowed()
    {
        // Arrange - delegation from user to self
        var sameUserId = new UserId(Guid.NewGuid());

        // Act
        var delegation = DelegatedAuthority.Create(
            sameUserId,
            sameUserId,
            _roleId,
            _scope,
            _now,
            _now.AddMonths(1));

        // Assert
        Assert.NotNull(delegation);
        Assert.Equal(sameUserId, delegation.DelegatorUserId);
        Assert.Equal(sameUserId, delegation.DelegatedToUserId);
    }

    [Fact]
    public void RevokeDelegation_AlreadyRevoked_ThrowsInvalidOperationException()
    {
        // Arrange
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            _now,
            _now.AddMonths(1));

        var revokerUserId = new UserId(Guid.NewGuid());
        delegation.Revoke(revokerUserId, "First revocation");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            delegation.Revoke(revokerUserId, "Second revocation"));

        Assert.NotNull(ex);
    }

    [Fact]
    public void MultipleScenario_CreateRevokeAndVerifyState()
    {
        // Scenario: Create a delegation, verify active state, revoke it, verify revoked state

        // Arrange
        var propertyIds = new[] { Guid.NewGuid() };
        var scope = DelegationScope.WithProperties(propertyIds);

        // Act 1: Create
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            scope,
            _now,
            _now.AddMonths(1));

        // Assert 1: Verify created
        Assert.Equal(DelegatedAuthorityStatus.Active, delegation.Status);
        Assert.Null(delegation.RevokedAtUtc);
        Assert.Null(delegation.RevokedBy);

        // Act 2: Add metadata
        delegation.ChangeDescription("Temporary delegation for Q4 operations");
        delegation.ChangeRemarks("Part of seasonal staffing plan");

        // Assert 2: Verify metadata
        Assert.NotNull(delegation.Description);
        Assert.NotNull(delegation.Remarks);

        // Act 3: Revoke
        var revokerUserId = new UserId(Guid.NewGuid());
        delegation.Revoke(revokerUserId, "End of assignment");

        // Assert 3: Verify revoked
        Assert.Equal(DelegatedAuthorityStatus.Revoked, delegation.Status);
        Assert.NotNull(delegation.RevokedAtUtc);
        Assert.Equal(revokerUserId, delegation.RevokedBy);
    }

    [Fact]
    public void DelegationValidator_SecondaryAuthorityCannotDelegatePrimaryAuthority()
    {
        // Arrange: Test the non-escalation invariant
        // A Secondary authority (level 3) attempting to delegate Primary authority (level 4) must be rejected.

        var delegatorUserId = Guid.NewGuid();
        var delegateeUserId = Guid.NewGuid();
        var primaryRoleId = Guid.NewGuid();  // Level 4 role
        var secondaryRoleId = Guid.NewGuid();  // Level 3 role

        // Create a mock authority level provider that maps roles to levels
        var authorityMap = new Dictionary<Guid, int>
        {
            { primaryRoleId, AuthorityLevels.PrimarySuperUser },      // Level 4
            { secondaryRoleId, AuthorityLevels.SecondarySuperUser }   // Level 3
        };
        var authorityProvider = new TestAuthorityLevelProvider(authorityMap);

        // Create the validator with the test provider
        var validator = new DelegationValidator(authorityProvider);

        // Create a delegation proposal requesting Primary level authority
        var proposal = new DelegationProposal(
            delegatorUserId: delegatorUserId,
            delegatedToUserId: delegateeUserId,
            delegatedRoleId: primaryRoleId,  // Requesting PRIMARY level (4)
            scope: DelegationScope.Unrestricted());

        // Create the delegator's effective authority at Secondary level (3)
        var delegatorAuthority = EffectiveAuthority.Create(
            userId: delegatorUserId,
            effectiveLevel: AuthorityLevels.SecondarySuperUser,  // Level 3
            roles: new[] { new RoleId(secondaryRoleId) },
            permissions: new[] { "delegation:create" },
            propertyScopes: Array.Empty<Guid>(),
            isInherentSuperUser: false);

        // Act: Validate the escalation attempt
        var result = validator.Validate(proposal, delegatorAuthority);

        // Assert: Must be rejected due to non-escalation invariant
        Assert.False(result.IsValid, "Validation should fail");
        Assert.Equal("delegation_exceeds_delegator_authority", result.ErrorCode);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Cannot delegate authority level 4", result.ErrorMessage);
        Assert.Contains("delegator effective level is 3", result.ErrorMessage);
    }

    [Fact]
    public void DelegationValidator_CannotDelegateOutsideEffectivePropertyScope()
    {
        // Arrange: Test property scope containment invariant
        // Delegator has property scope [PropertyA]
        // Requested delegation includes [PropertyB] which is outside delegator's scope
        // Expected: Rejection with "scope_expansion"

        var delegatorUserId = Guid.NewGuid();
        var delegateeUserId = Guid.NewGuid();
        var delegatedRoleId = Guid.NewGuid();

        var propertyA = Guid.NewGuid();  // Delegator's authorized property
        var propertyB = Guid.NewGuid();  // Outside delegator's scope

        var authorityMap = new Dictionary<Guid, int>
        {
            { delegatedRoleId, AuthorityLevels.Admin }  // Level 2
        };
        var authorityProvider = new TestAuthorityLevelProvider(authorityMap);
        var validator = new DelegationValidator(authorityProvider);

        // Create delegator's effective authority with scope [PropertyA]
        var delegatorAuthority = EffectiveAuthority.Create(
            userId: delegatorUserId,
            effectiveLevel: AuthorityLevels.SecondarySuperUser,  // Level 3, can delegate
            roles: new[] { new RoleId(Guid.NewGuid()) },
            permissions: new[] { "delegation:create" },
            propertyScopes: new[] { propertyA },  // Only PropertyA
            isInherentSuperUser: false);

        // Create proposal requesting delegation over PropertyB (outside scope)
        var proposal = new DelegationProposal(
            delegatorUserId: delegatorUserId,
            delegatedToUserId: delegateeUserId,
            delegatedRoleId: delegatedRoleId,
            scope: DelegationScope.WithProperties(new[] { propertyB }));  // PropertyB not in delegator scope

        // Act
        var result = validator.Validate(proposal, delegatorAuthority);

        // Assert: Must be rejected due to scope containment violation
        Assert.False(result.IsValid, "Validation should fail");
        Assert.Equal("scope_expansion", result.ErrorCode);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("not in delegator's scope", result.ErrorMessage);
    }

    [Fact]
    public void DelegationValidator_CannotDelegateBeyondEffectiveAuthorityPeriod()
    {
        // Arrange: Test temporal containment invariant
        // Delegator's authority period: T1 → T2
        // Requested delegation: T1 → T3 (where T3 > T2)
        // Expected: Rejection with "temporal_expansion" because delegation outlives delegator

        var delegatorUserId = Guid.NewGuid();
        var delegateeUserId = Guid.NewGuid();
        var delegatedRoleId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var delegatorAuthorityEnd = now.AddMonths(6);
        var delegationEnd = delegatorAuthorityEnd.AddMonths(3);  // Exceeds delegator's authority end

        var authorityMap = new Dictionary<Guid, int>
        {
            { delegatedRoleId, AuthorityLevels.Admin }
        };
        var authorityProvider = new TestAuthorityLevelProvider(authorityMap);
        var validator = new DelegationValidator(authorityProvider);

        // Delegator has SecondarySuperUser level (level 3) with authority expiring at delegatorAuthorityEnd
        var delegatorAuthority = EffectiveAuthority.Create(
            userId: delegatorUserId,
            effectiveLevel: AuthorityLevels.SecondarySuperUser,  // Level 3 - can delegate
            roles: new[] { new RoleId(Guid.NewGuid()) },
            permissions: new[] { "delegation:create" },
            propertyScopes: Array.Empty<Guid>(),
            isInherentSuperUser: false);

        // Proposed delegation extends beyond delegator's authority end
        var proposal = new DelegationProposal(
            delegatorUserId: delegatorUserId,
            delegatedToUserId: delegateeUserId,
            delegatedRoleId: delegatedRoleId,
            scope: DelegationScope.Unrestricted(),
            effectiveFromUtc: now,
            effectiveToUtc: delegationEnd);  // Beyond delegator authority end

        // Act: Use new ValidateWithTemporalBounds method
        var result = validator.ValidateWithTemporalBounds(
            proposal,
            delegatorAuthority,
            delegatorAuthorityEffectiveToUtc: delegatorAuthorityEnd);

        // Assert: Must be rejected due to temporal expansion
        Assert.False(result.IsValid, "Validation should fail");
        Assert.Equal("temporal_expansion", result.ErrorCode);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("exceeds delegator authority end", result.ErrorMessage);
    }

    [Fact]
    public void DelegatedSecondaryAuthority_MustRemainTemporallyBounded()
    {
        // SECURITY TEST: Proves delegated authority IS temporally bounded.
        //
        // Scenario:
        // - Inherent Primary delegates Secondary authority to UserA
        // - UserA has effective level = 3 (Secondary), but IsInherentSuperUser = false (delegated)
        // - UserA attempts to delegate beyond their delegated temporal bounds
        // - Expected: REJECTION (because delegated authority must remain bounded)
        //
        // This proves that numeric level alone does NOT grant exemption from temporal validation.
        // Only inherent Primary authority can bypass temporal containment.

        var primaryUserId = Guid.NewGuid();  // Inherent Primary
        var delegatedSecondaryUserId = Guid.NewGuid();  // Receives delegated Secondary (level 3)
        var furtherDelegateeId = Guid.NewGuid();  // UserA tries to delegate to this user
        var adminRoleId = Guid.NewGuid();

        var now = DateTime.UtcNow;
        var delegatedSecondaryAuthEnd = now.AddMonths(3);  // Delegated Secondary authority expires in 3 months
        var proposedDelegationEnd = delegatedSecondaryAuthEnd.AddMonths(2);  // UserA requests delegation beyond that

        var authorityMap = new Dictionary<Guid, int>
        {
            { adminRoleId, AuthorityLevels.Admin }
        };
        var authorityProvider = new TestAuthorityLevelProvider(authorityMap);
        var validator = new DelegationValidator(authorityProvider);

        // UserA has delegated Secondary authority (level 3)
        // IsInherentSuperUser = false (explicitly set to show it's delegated, not inherent)
        var delegatedSecondaryAuthority = EffectiveAuthority.Create(
            userId: delegatedSecondaryUserId,
            effectiveLevel: AuthorityLevels.SecondarySuperUser,  // Level 3
            roles: new[] { new RoleId(Guid.NewGuid()) },
            permissions: new[] { "delegation:create" },
            propertyScopes: Array.Empty<Guid>(),
            isInherentSuperUser: false);  // CRITICAL: delegated, not inherent

        // UserA proposes delegation beyond their delegated temporal bounds
        var proposal = new DelegationProposal(
            delegatorUserId: delegatedSecondaryUserId,
            delegatedToUserId: furtherDelegateeId,
            delegatedRoleId: adminRoleId,
            scope: DelegationScope.Unrestricted(),
            effectiveFromUtc: now,
            effectiveToUtc: proposedDelegationEnd);  // Extends beyond delegated authority end

        // Act: Validate WITH delegated authority temporal bounds
        var result = validator.ValidateWithTemporalBounds(
            proposal,
            delegatedSecondaryAuthority,
            delegatorAuthorityEffectiveToUtc: delegatedSecondaryAuthEnd);

        // Assert: Must be rejected due to temporal expansion
        // This proves delegated authority CANNOT bypass temporal validation even at high levels
        Assert.False(result.IsValid, "Delegated authority must be temporally bounded");
        Assert.Equal("temporal_expansion", result.ErrorCode);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("exceeds delegator authority end", result.ErrorMessage);
    }

    [Fact]
    public void InherentPrimaryAuthority_IsExemptFromTemporalBounds()
    {
        // SECURITY TEST: Proves inherent Primary authority IS exempt from temporal containment.
        //
        // Scenario:
        // - Inherent Primary with finite delegated authority period
        // - Primary attempts unlimited delegation (beyond their own temporal bounds)
        // - Expected: ACCEPTED (because inherent Primary is unrestricted)
        //
        // This proves the inverse: only inherent Primary gets the exemption,
        // based on IsInherentSuperUser = true, not just EffectiveLevel >= 4.

        var primaryUserId = Guid.NewGuid();
        var delegateeId = Guid.NewGuid();
        var secondaryRoleId = Guid.NewGuid();

        var now = DateTime.UtcNow;
        var primaryAuthorityEnd = now.AddMonths(6);  // Even if Primary's authority has an end date
        var proposedDelegationEnd = now.AddYears(10);  // Unlimited delegation (way beyond)

        var authorityMap = new Dictionary<Guid, int>
        {
            { secondaryRoleId, AuthorityLevels.SecondarySuperUser }
        };
        var authorityProvider = new TestAuthorityLevelProvider(authorityMap);
        var validator = new DelegationValidator(authorityProvider);

        // Inherent Primary authority
        var inheritedPrimaryAuthority = EffectiveAuthority.Create(
            userId: primaryUserId,
            effectiveLevel: AuthorityLevels.PrimarySuperUser,  // Level 4
            roles: new[] { new RoleId(Guid.NewGuid()) },
            permissions: new[] { "delegation:create", "unrestricted:access" },
            propertyScopes: Array.Empty<Guid>(),
            isInherentSuperUser: true);  // CRITICAL: inherent, not delegated

        // Primary proposes delegation far beyond any temporal bounds
        var proposal = new DelegationProposal(
            delegatorUserId: primaryUserId,
            delegatedToUserId: delegateeId,
            delegatedRoleId: secondaryRoleId,
            scope: DelegationScope.Unrestricted(),
            effectiveFromUtc: now,
            effectiveToUtc: proposedDelegationEnd);  // 10 years in future

        // Act: Validate WITH temporal bounds that would normally reject
        var result = validator.ValidateWithTemporalBounds(
            proposal,
            inheritedPrimaryAuthority,
            delegatorAuthorityEffectiveToUtc: primaryAuthorityEnd);  // Only 6 months

        // Assert: Must be ACCEPTED because inherent Primary bypasses temporal bounds
        Assert.True(result.IsValid,
            "Inherent Primary authority must be exempt from temporal bounds");
    }

    /// <summary>
    /// Test implementation of IAuthorityLevelProvider for validation testing.
    /// </summary>
    private sealed class TestAuthorityLevelProvider : IAuthorityLevelProvider
    {
        private readonly Dictionary<Guid, int> _roleAuthorityMap;

        public TestAuthorityLevelProvider(Dictionary<Guid, int> roleAuthorityMap)
        {
            _roleAuthorityMap = roleAuthorityMap ?? throw new ArgumentNullException(nameof(roleAuthorityMap));
        }

        public int GetAuthorityLevel(Guid roleId)
        {
            if (_roleAuthorityMap.TryGetValue(roleId, out var level))
            {
                return level;
            }
            throw new KeyNotFoundException($"Role {roleId} not found in authority map");
        }
    }
}
