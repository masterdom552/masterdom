using Masterdom.Core.Security;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class NotificationAuthorizationService : INotificationAuthorizationService
{
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public NotificationAuthorizationService(ICurrentUserAccessor currentUserAccessor)
    {
        _currentUserAccessor = currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
    }

    public void EnsureAuthorized(string eventCode, Guid recipientId)
    {
        _ = eventCode;
        _ = recipientId;

        var user = _currentUserAccessor.GetCurrentUser();
        if (!user.IsAuthenticated)
        {
            throw new InvalidOperationException("Notification request requires an authenticated user.");
        }

        if (user.IsInRole(MasterdomRoles.SuperUser))
        {
            return;
        }

        if (!user.HasPermission("notifications.send"))
        {
            throw new InvalidOperationException("The current user does not have notification permissions.");
        }
    }
}
