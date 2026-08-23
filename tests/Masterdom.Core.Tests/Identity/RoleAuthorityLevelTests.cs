using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Security;

namespace Masterdom.Core.Tests.Identity;

public sealed class RoleAuthorityLevelTests
{
    [Theory]
    [InlineData(AuthorityLevels.PrimarySuperUser)]
    [InlineData(AuthorityLevels.SecondarySuperUser)]
    [InlineData(AuthorityLevels.Admin)]
    [InlineData(AuthorityLevels.Tenant)]
    public void Create_WithValidLevel_ShouldSucceed(int level)
    {
        var authorityLevel = RoleAuthorityLevel.Create(level);

        Assert.Equal(level, authorityLevel.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void Create_WithInvalidLevel_ShouldThrow(int level)
    {
        Assert.Throws<ArgumentException>(() => RoleAuthorityLevel.Create(level));
    }

    [Fact]
    public void Create_ShouldReturnConsistentSingletonPerLevel()
    {
        var first = RoleAuthorityLevel.Create(AuthorityLevels.SecondarySuperUser);
        var second = RoleAuthorityLevel.Create(AuthorityLevels.SecondarySuperUser);

        Assert.Same(first, second);
        Assert.Equal(RoleAuthorityLevel.SecondarySuperUser, first);
    }

    [Fact]
    public void Equality_ShouldBeValueBased()
    {
        Assert.Equal(RoleAuthorityLevel.PrimarySuperUser, RoleAuthorityLevel.Create(AuthorityLevels.PrimarySuperUser));
        Assert.NotEqual(RoleAuthorityLevel.PrimarySuperUser, RoleAuthorityLevel.Tenant);
    }
}
