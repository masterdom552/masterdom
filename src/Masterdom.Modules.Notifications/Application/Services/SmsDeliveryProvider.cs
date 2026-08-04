using Masterdom.Modules.Notifications.Application.Models;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class SmsDeliveryProvider : IDeliveryProvider
{
    public DeliveryChannel Channel => DeliveryChannel.Sms;

    public bool Deliver(GeneratedNotification notification)
    {
        _ = notification;
        return true;
    }
}
