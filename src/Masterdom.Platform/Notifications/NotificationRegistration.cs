namespace Masterdom.Platform.Notifications;

public sealed record NotificationRegistration(
    string EventCode,
    int Version,
    string ReadModelKey,
    string TemplateCode,
    string RecipientResolver,
    IReadOnlyCollection<string> DeliveryChannels,
    string Priority,
    int RetryMaxAttempts,
    int RetryDelaySeconds,
    string SchedulingPolicy,
    bool AuditEnabled);
