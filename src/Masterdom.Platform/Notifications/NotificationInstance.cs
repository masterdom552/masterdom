namespace Masterdom.Platform.Notifications;

public sealed record NotificationInstance(
    string NotificationId,
    string EventCode,
    Guid RecipientId,
    string Subject,
    string Body,
    IReadOnlyCollection<string> Channels,
    string Category,
    string Priority,
    DateTime ScheduledAtUtc,
    int AttemptCount,
    bool IsDelivered,
    string Status);
