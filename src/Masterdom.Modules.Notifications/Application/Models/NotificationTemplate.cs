namespace Masterdom.Modules.Notifications.Application.Models;

public sealed record NotificationTemplate(
    string TemplateCode,
    string SubjectTemplate,
    string BodyTemplate,
    NotificationCategory Category,
    NotificationPriority Priority,
    IReadOnlyCollection<DeliveryChannel> DefaultChannels);
