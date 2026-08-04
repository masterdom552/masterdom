namespace Masterdom.Platform.Notifications;

public interface INotificationRegistry
{
    NotificationRegistration Resolve(string eventCode);

    IReadOnlyCollection<NotificationRegistration> GetAll();
}
