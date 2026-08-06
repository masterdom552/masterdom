using Masterdom.Core.Identity.Entities.Role;

namespace Masterdom.Core.Tests.Identity;

public sealed class RoleTests
{
    [Fact]
    public void Create_ShouldInitializeActiveRole()
    {
        var role = Role.Create(
            RoleCode.Create("ROLE-ADMIN"),
            RoleName.Create("Platform Administrator"));

        Assert.Equal(RoleCode.Create("ROLE-ADMIN"), role.Code);
        Assert.Equal(RoleName.Create("Platform Administrator"), role.Name);
        Assert.Equal(RoleStatus.Active, role.Status);
        Assert.False(role.IsHidden);
        Assert.Equal(0, role.DisplayOrder);
    }
}
