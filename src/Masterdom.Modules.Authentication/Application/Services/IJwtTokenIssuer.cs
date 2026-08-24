using Masterdom.Modules.Authentication.Application.Models;

namespace Masterdom.Modules.Authentication.Application.Services;

public interface IJwtTokenIssuer
{
    LoginResult Issue(
        Guid userId,
        string username,
        Guid? personId,
        IReadOnlyCollection<Guid> ownedPropertyIds);
}
