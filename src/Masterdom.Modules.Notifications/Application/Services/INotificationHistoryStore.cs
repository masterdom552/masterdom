using Masterdom.Modules.Notifications.Application.Models;

namespace Masterdom.Modules.Notifications.Application.Services;

public interface INotificationHistoryStore
{
    void Save(NotificationHistoryEntry entry);

    IReadOnlyCollection<NotificationHistoryEntry> GetByRecipient(Guid recipientId, int page, int pageSize);
}
