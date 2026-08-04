using Masterdom.Modules.Notifications.Application.Models;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class PushDeliveryProvider : IDeliveryProvider
{
    public DeliveryChannel Channel => DeliveryChannel.Push;

    public bool Deliver(GeneratedNotification notification)
    {
        _ = notification;
        return true;
    }
}
