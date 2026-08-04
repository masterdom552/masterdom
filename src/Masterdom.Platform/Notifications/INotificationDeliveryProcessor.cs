namespace Masterdom.Platform.Notifications;

public interface INotificationDeliveryProcessor
{
    NotificationInstance Deliver(NotificationDeliveryEnvelope envelope);
}
