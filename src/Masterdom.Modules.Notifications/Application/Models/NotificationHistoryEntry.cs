namespace Masterdom.Modules.Notifications.Application.Models;

public sealed record NotificationHistoryEntry(
    string NotificationId,
    string EventCode,
    Guid RecipientId,
    DateTime RequestedAtUtc,
    DateTime LastAttemptAtUtc,
    int AttemptCount,
    bool IsDelivered,
    string Status,
    string AuditTrail);
