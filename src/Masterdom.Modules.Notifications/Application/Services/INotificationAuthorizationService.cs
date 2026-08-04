namespace Masterdom.Modules.Notifications.Application.Services;

public interface INotificationAuthorizationService
{
    void EnsureAuthorized(string eventCode, Guid recipientId);
}
