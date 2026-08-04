using Masterdom.Modules.Notifications.Application.Models;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class EmailDeliveryProvider : IDeliveryProvider
{
    public DeliveryChannel Channel => DeliveryChannel.Email;

    public bool Deliver(GeneratedNotification notification)
    {
        _ = notification;
        return true;
    }
}
