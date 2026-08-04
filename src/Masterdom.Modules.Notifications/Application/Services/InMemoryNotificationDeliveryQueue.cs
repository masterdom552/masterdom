using Masterdom.Platform.Notifications;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class InMemoryNotificationDeliveryQueue : INotificationDeliveryQueue
{
    private readonly Queue<NotificationDeliveryEnvelope> _queue = new();

    public void Enqueue(NotificationDeliveryEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        _queue.Enqueue(envelope);
    }

    public NotificationDeliveryEnvelope? Dequeue()
    {
        return _queue.Count == 0 ? null : _queue.Dequeue();
    }
}
