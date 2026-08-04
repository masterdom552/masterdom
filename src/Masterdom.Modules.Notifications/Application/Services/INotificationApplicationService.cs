using Masterdom.Modules.Notifications.Application.Models;

namespace Masterdom.Modules.Notifications.Application.Services;

public interface INotificationApplicationService
{
    GeneratedNotification Generate(
        string eventCode,
        Guid recipientId,
        DateTime requestedAtUtc,
        IReadOnlyDictionary<string, string> parameters,
        DateTime? requestedDeliveryAtUtc);

    IReadOnlyCollection<NotificationHistoryEntry> History(Guid recipientId, int page, int pageSize);
}
