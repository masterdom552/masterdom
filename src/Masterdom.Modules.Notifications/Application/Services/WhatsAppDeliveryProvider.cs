using Masterdom.Modules.Notifications.Application.Models;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class WhatsAppDeliveryProvider : IDeliveryProvider
{
    public DeliveryChannel Channel => DeliveryChannel.WhatsApp;

    public bool Deliver(GeneratedNotification notification)
    {
        _ = notification;
        return true;
    }
}
