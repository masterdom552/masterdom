using Masterdom.Core.Identity.Entities.Role;

namespace Masterdom.Core.Tests.Identity;

public sealed class RoleTests
{
    [Fact]
    public void Create_ShouldInitializeActiveRole()
    {
        var role = Role.Create(
            RoleCode.Create("ROLE-ADMIN"),
            RoleName.Create("Platform Administrator"),
            RoleAuthorityLevel.Admin);

        Assert.Equal(RoleCode.Create("ROLE-ADMIN"), role.Code);
        Assert.Equal(RoleName.Create("Platform Administrator"), role.Name);
        Assert.Equal(RoleAuthorityLevel.Admin, role.AuthorityLevel);
        Assert.Equal(RoleStatus.Active, role.Status);
        Assert.False(role.IsHidden);
        Assert.Equal(0, role.DisplayOrder);
    }

    [Fact]
    public void Reclassify_ShouldChangeAuthorityLevel()
    {
        var role = Role.Create(
            RoleCode.Create("ROLE-ADMIN"),
            RoleName.Create("Platform Administrator"),
            RoleAuthorityLevel.Admin);

        role.Reclassify(RoleAuthorityLevel.SecondarySuperUser);

        Assert.Equal(RoleAuthorityLevel.SecondarySuperUser, role.AuthorityLevel);
    }

    [Fact]
    public void Reclassify_ToSameLevel_ShouldBeNoOp()
    {
        var role = Role.Create(
            RoleCode.Create("ROLE-ADMIN"),
            RoleName.Create("Platform Administrator"),
            RoleAuthorityLevel.Admin);

        role.Reclassify(RoleAuthorityLevel.Admin);

        Assert.Equal(RoleAuthorityLevel.Admin, role.AuthorityLevel);
    }

    [Fact]
    public void Reclassify_NullLevel_ShouldThrow()
    {
        var role = Role.Create(
            RoleCode.Create("ROLE-ADMIN"),
            RoleName.Create("Platform Administrator"),
            RoleAuthorityLevel.Admin);

        Assert.Throws<ArgumentNullException>(() => role.Reclassify(null!));
    }
}
