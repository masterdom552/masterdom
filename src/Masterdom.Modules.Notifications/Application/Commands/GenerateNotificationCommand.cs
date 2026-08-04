namespace Masterdom.Modules.Notifications.Application.Commands;

public sealed record GenerateNotificationCommand(
    string EventCode,
    Guid RecipientId,
    DateTime RequestedAtUtc,
    IReadOnlyDictionary<string, string> Parameters,
    DateTime? RequestedDeliveryAtUtc);
