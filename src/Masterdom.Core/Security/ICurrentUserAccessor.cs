namespace Masterdom.Core.Security;

/// <summary>
/// Provides the caller projected into the current execution scope.
/// </summary>
public interface ICurrentUserAccessor
{
    CurrentUser GetCurrentUser();
}
