namespace Masterdom.Modules.Notifications.Application.Models;

public sealed record GeneratedNotification(
    string NotificationId,
    string EventCode,
    Guid RecipientId,
    NotificationCategory Category,
    NotificationPriority Priority,
    string Subject,
    string Body,
    IReadOnlyCollection<DeliveryChannel> Channels,
    DateTime ScheduledAtUtc,
    int AttemptCount,
    bool IsDelivered,
    string Status);
