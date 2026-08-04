using Masterdom.Core.Security;

namespace Masterdom.Infrastructure.Security;

internal sealed class AnonymousCurrentUserAccessor : ICurrentUserAccessor
{
    public CurrentUser GetCurrentUser() => CurrentUser.Anonymous;
}
