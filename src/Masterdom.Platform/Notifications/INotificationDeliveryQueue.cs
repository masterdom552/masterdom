namespace Masterdom.Platform.Notifications;

public interface INotificationDeliveryQueue
{
    void Enqueue(NotificationDeliveryEnvelope envelope);

    NotificationDeliveryEnvelope? Dequeue();
}
