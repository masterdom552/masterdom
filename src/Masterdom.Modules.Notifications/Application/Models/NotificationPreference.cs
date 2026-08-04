namespace Masterdom.Modules.Notifications.Application.Models;

public sealed record NotificationPreference(
    Guid RecipientId,
    IReadOnlyCollection<DeliveryChannel> Channels,
    bool IsEnabled,
    int QuietHoursStart,
    int QuietHoursEnd);
