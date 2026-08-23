using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.ValueObjects;
using Masterdom.Core.Security;
using Xunit;

namespace Masterdom.Core.Tests.Identity.Delegation;

/// <summary>
/// Tests for DelegatedAuthority aggregate invariants.
/// </summary>
public class DelegatedAuthorityTests
{
    private readonly UserId _delegatorUserId = new(Guid.NewGuid());
    private readonly UserId _delegateeUserId = new(Guid.NewGuid());
    private readonly RoleId _roleId = new(Guid.NewGuid());
    private readonly DelegationScope _scope = DelegationScope.Unrestricted();
    private readonly DateTime _effectiveFromUtc = DateTime.UtcNow;

    [Fact]
    public void Create_WithValidParameters_Succeeds()
    {
        // Act
        var result = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            _effectiveFromUtc,
            null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_delegatorUserId, result.DelegatorUserId);
        Assert.Equal(_delegateeUserId, result.DelegatedToUserId);
        Assert.Equal(_roleId, result.DelegatedRoleId);
        Assert.Equal(DelegatedAuthorityStatus.Active, result.Status);
        Assert.Equal(_effectiveFromUtc, result.EffectiveFromUtc);
    }

    [Fact]
    public void Create_WithNullDelegator_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            DelegatedAuthority.Create(
                null!,
                _delegateeUserId,
                _roleId,
                _scope,
                _effectiveFromUtc,
                null));
    }

    [Fact]
    public void Create_WithNullDelegatee_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            DelegatedAuthority.Create(
                _delegatorUserId,
                null!,
                _roleId,
                _scope,
                _effectiveFromUtc,
                null));
    }

    [Fact]
    public void Create_WithNullRole_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            DelegatedAuthority.Create(
                _delegatorUserId,
                _delegateeUserId,
                null!,
                _scope,
                _effectiveFromUtc,
                null));
    }

    [Fact]
    public void Create_WithNullScope_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            DelegatedAuthority.Create(
                _delegatorUserId,
                _delegateeUserId,
                _roleId,
                null!,
                _effectiveFromUtc,
                null));
    }

    [Fact]
    public void Create_WithEffectiveToBeforeEffectiveFrom_Throws()
    {
        // Arrange
        var effectiveFrom = DateTime.UtcNow;
        var effectiveTo = effectiveFrom.AddHours(-1);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            DelegatedAuthority.Create(
                _delegatorUserId,
                _delegateeUserId,
                _roleId,
                _scope,
                effectiveFrom,
                effectiveTo));

        Assert.Contains("EffectiveToUtc", ex.Message);
    }

    [Fact]
    public void Revoke_Active_TransitionsToRevoked()
    {
        // Arrange
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            _effectiveFromUtc,
            null);

        var revokerUserId = new UserId(Guid.NewGuid());
        var reason = "Testing revocation";

        // Act
        delegation.Revoke(revokerUserId, reason);

        // Assert
        Assert.Equal(DelegatedAuthorityStatus.Revoked, delegation.Status);
        Assert.NotNull(delegation.RevokedAtUtc);
        Assert.Equal(revokerUserId, delegation.RevokedBy);
        Assert.Equal(reason, delegation.RevocationReason);
    }

    [Fact]
    public void Revoke_AlreadyRevoked_Throws()
    {
        // Arrange
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            _effectiveFromUtc,
            null);

        var revokerUserId = new UserId(Guid.NewGuid());
        delegation.Revoke(revokerUserId, "First revocation");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            delegation.Revoke(revokerUserId, "Second revocation"));

        Assert.Contains("already revoked", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IsEffective_BeforeEffectiveFromUtc_ReturnsFalse()
    {
        // Arrange
        var effectiveFrom = DateTime.UtcNow.AddHours(1);
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            effectiveFrom,
            null);

        var checkTime = effectiveFrom.AddMilliseconds(-1);

        // Act
        var result = delegation.IsEffective(checkTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEffective_AtEffectiveFromUtc_ReturnsTrue()
    {
        // Arrange
        var effectiveFrom = DateTime.UtcNow;
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            effectiveFrom,
            null);

        // Act
        var result = delegation.IsEffective(effectiveFrom);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEffective_AfterEffectiveToUtc_ReturnsFalse()
    {
        // Arrange
        var effectiveFrom = DateTime.UtcNow;
        var effectiveTo = effectiveFrom.AddHours(1);
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            effectiveFrom,
            effectiveTo);

        var checkTime = effectiveTo.AddMilliseconds(1);

        // Act
        var result = delegation.IsEffective(checkTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEffective_AtEffectiveToUtc_ReturnsTrue()
    {
        // Arrange
        var effectiveFrom = DateTime.UtcNow;
        var effectiveTo = effectiveFrom.AddHours(1);
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            effectiveFrom,
            effectiveTo);

        // Act
        var result = delegation.IsEffective(effectiveTo);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEffective_WhenRevoked_ReturnsFalse()
    {
        // Arrange
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            _effectiveFromUtc,
            null);

        delegation.Revoke(new UserId(Guid.NewGuid()), "test");

        // Act
        var result = delegation.IsEffective(DateTime.UtcNow);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ChangeDescription_UpdatesDescription()
    {
        // Arrange
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            _effectiveFromUtc,
            null);

        var newDescription = "Updated description";

        // Act
        delegation.ChangeDescription(newDescription);

        // Assert
        Assert.Equal(newDescription, delegation.Description);
    }

    [Fact]
    public void ChangeRemarks_UpdatesRemarks()
    {
        // Arrange
        var delegation = DelegatedAuthority.Create(
            _delegatorUserId,
            _delegateeUserId,
            _roleId,
            _scope,
            _effectiveFromUtc,
            null);

        var newRemarks = "Updated remarks";

        // Act
        delegation.ChangeRemarks(newRemarks);

        // Assert
        Assert.Equal(newRemarks, delegation.Remarks);
    }

    [Fact]
    public void DelegatedAuthorityId_CanBeCreated()
    {
        // Act
        var id = DelegatedAuthorityId.New();

        // Assert
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void DelegatedAuthorityId_CanBeConvertedFromGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = DelegatedAuthorityId.From(guid);

        // Assert
        Assert.Equal(guid, id.Value);
    }
}
