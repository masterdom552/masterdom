using Masterdom.Core.Identity.ValueObjects;
using Xunit;

namespace Masterdom.Core.Tests.Identity.Delegation;

/// <summary>
/// Tests for DelegationScope value object.
/// </summary>
public class DelegationScopeTests
{
    [Fact]
    public void Unrestricted_HasNoPropertyRestriction()
    {
        // Act
        var scope = DelegationScope.Unrestricted();

        // Assert
        Assert.Null(scope.PropertyIds);
        Assert.Null(scope.EffectiveLevel);
    }

    [Fact]
    public void Unrestricted_ContainsAnyProperty()
    {
        // Arrange
        var scope = DelegationScope.Unrestricted();
        var anyPropertyId = Guid.NewGuid();

        // Act
        var result = scope.ContainsProperty(anyPropertyId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WithProperties_CreatesPropertyScoped()
    {
        // Arrange
        var propertyIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var scope = DelegationScope.WithProperties(propertyIds);

        // Assert
        Assert.NotNull(scope.PropertyIds);
        Assert.Equal(2, scope.PropertyIds.Count);
    }

    [Fact]
    public void WithProperties_EmptyArray_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            DelegationScope.WithProperties(Array.Empty<Guid>()));
    }

    [Fact]
    public void WithProperties_ContainsIncludedProperty()
    {
        // Arrange
        var propertyId1 = Guid.NewGuid();
        var propertyId2 = Guid.NewGuid();
        var propertyId3 = Guid.NewGuid();

        var scope = DelegationScope.WithProperties(new[] { propertyId1, propertyId2 });

        // Act & Assert
        Assert.True(scope.ContainsProperty(propertyId1));
        Assert.True(scope.ContainsProperty(propertyId2));
        Assert.False(scope.ContainsProperty(propertyId3));
    }

    [Fact]
    public void WithEffectiveLevel_CreatesLevelCapped()
    {
        // Arrange
        var level = 3;

        // Act
        var scope = DelegationScope.WithEffectiveLevel(level);

        // Assert
        Assert.Null(scope.PropertyIds);
        Assert.Equal(level, scope.EffectiveLevel);
    }

    [Fact]
    public void WithEffectiveLevel_LessThanOne_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            DelegationScope.WithEffectiveLevel(0));
    }

    [Fact]
    public void WithEffectiveLevel_IsLevelWithinScope_AtLevel()
    {
        // Arrange
        var scope = DelegationScope.WithEffectiveLevel(3);

        // Act & Assert
        Assert.True(scope.IsLevelWithinScope(3));
    }

    [Fact]
    public void WithEffectiveLevel_IsLevelWithinScope_BelowLevel()
    {
        // Arrange
        var scope = DelegationScope.WithEffectiveLevel(3);

        // Act & Assert
        Assert.True(scope.IsLevelWithinScope(2));
    }

    [Fact]
    public void WithEffectiveLevel_IsLevelWithinScope_AboveLevel()
    {
        // Arrange
        var scope = DelegationScope.WithEffectiveLevel(3);

        // Act & Assert
        Assert.False(scope.IsLevelWithinScope(4));
    }

    [Fact]
    public void WithPropertiesAndLevel_CreatesBothConstraints()
    {
        // Arrange
        var propertyIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var level = 3;

        // Act
        var scope = DelegationScope.WithPropertiesAndLevel(propertyIds, level);

        // Assert
        Assert.NotNull(scope.PropertyIds);
        Assert.Equal(level, scope.EffectiveLevel);
    }

    [Fact]
    public void Unrestricted_IsLevelWithinScope_ReturnsTrue()
    {
        // Arrange
        var scope = DelegationScope.Unrestricted();

        // Act & Assert
        Assert.True(scope.IsLevelWithinScope(1));
        Assert.True(scope.IsLevelWithinScope(2));
        Assert.True(scope.IsLevelWithinScope(3));
        Assert.True(scope.IsLevelWithinScope(4));
    }
}
