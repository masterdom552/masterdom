using Masterdom.Modules.Notifications.Application.Models;

namespace Masterdom.Modules.Notifications.Application.Services;

public interface IDeliveryProvider
{
    DeliveryChannel Channel { get; }

    bool Deliver(GeneratedNotification notification);
}
