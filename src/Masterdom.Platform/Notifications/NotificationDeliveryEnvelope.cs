namespace Masterdom.Platform.Notifications;

public sealed record NotificationDeliveryEnvelope(
    NotificationInstance Instance,
    int MaxAttempts,
    int RetryDelaySeconds);
