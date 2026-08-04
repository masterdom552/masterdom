using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Core.Identity.Entities.User;

namespace Masterdom.Core.Tests.Identity;

public sealed class UserTests
{
    [Fact]
    public void Create_ShouldInitializeActiveUser()
    {
        var code = UserCode.Create("USR-001");
        var identityProfile = IdentityProfile.Create(
            IdentityProfileCode.Create("IP-001"),
            IdentityProfileType.Person);

        var user = User.Create(
            code,
            identityProfile.Id,
            Username.Create("alice"));

        Assert.Equal(code, user.Code);
        Assert.Equal(identityProfile.Id, user.IdentityProfileId);
        Assert.Equal(Username.Create("alice"), user.Username);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.False(user.IsHidden);
        Assert.Equal(0, user.DisplayOrder);
    }
}
